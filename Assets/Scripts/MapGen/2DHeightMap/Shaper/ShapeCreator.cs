using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

//General class that controls shape creation and placement.
public static class ShapeCreator 
{
    public static float[,] PreMap;
    public static int[,] TempLand;

    static DensityMap DenistyController;
    static PreMapGrid GridController;
    //Main function that creates and places shapes.
    public static float[,] CreateShapes()
    {
        //Basic setup as well as inline TaskFactory creation.
        PreMap = new float[Options.MapSize, Options.MapSize];
        DenistyController = new DensityMap();
        TempLand = new int[Options.MapSize, Options.MapSize];
        List<Shape> shapes = new List<Shape>();
        GridController = new PreMapGrid();
        CancellationTokenSource cts = new CancellationTokenSource();
        TaskFactory factory = new TaskFactory
           (
           cts.Token,
           TaskCreationOptions.PreferFairness,
           TaskContinuationOptions.ExecuteSynchronously,
           new SingleThreadSchedueler()
           );
        List<Task> tasks = new List<Task>();

        //Initial shape creation in individual cells on the map.
        for (int i = 0; i < Options.ShapeCount; i++)
        {
            shapes.Add(AddShape(i+1));
        }

        //Clearing the shape list in case the requested shape count was not able to generate.
        while (shapes.Contains(null))
        {
            shapes.Remove(null);
        }

        //Individual finished shape placement based on the amount of free space.
        for (int i = 0; i < shapes.Count; i++)
        {
            //Collecting shape data.
            RemoveShapeFromTempLand(shapes[i]);
            Dot start = shapes[i].OuterBox.Start;
            Dot end = shapes[i].OuterBox.End;
            int width = shapes[i].OuterBox.XSize;
            int height = shapes[i].OuterBox.YSize;
            int minYMove = 0;
            bool botEnd = false;
            //Checking the amount of free space to move the shape on the Y axis.
            for (int y = start.Y; y > 0; y--)
            {
                for (int x = start.X; x < start.X + width; x++)
                {
                    if (TempLand[x, y] != 0)
                    {
                        botEnd = true;
                        break;
                    }
                }
                if (botEnd)
                {
                    break;
                }
                minYMove--;
            }
            int maxYMove = 0;
            bool topEnd = false;
            for (int y = end.Y; y < TempLand.GetLength(1); y++)
            {
                for (int x = start.X; x < start.X + width; x++)
                {
                    if (TempLand[x, y] != 0)
                    {
                        topEnd = true;
                        break;
                    }
                }
                if (topEnd)
                {
                    break;
                }
                maxYMove++;
            }
            if (minYMove < -4)
            {
                minYMove += 4;
            }
            if (maxYMove > 4)
            {
                maxYMove -= 4;
            }

            //Moving the shape on the Y axis within collected limitations.
            int endYMove = Seed.seed.Next(minYMove, maxYMove);
            shapes[i].MoveBy(new Dot(0, endYMove));

            //Recollecting shape data after movement.
            start = shapes[i].OuterBox.Start;
            end = shapes[i].OuterBox.End;

            //Checking the amount of free space to move the shape on the X axis.
            int minXMove = 0;
            bool leftEnd = false;
            for (int x = start.X; x > 0; x--)
            {
                for (int y = start.Y; y < start.Y + height; y++)
                {
                    if (TempLand[x, y] != 0)
                    {
                        leftEnd = true;
                        break;
                    }
                }
                if (leftEnd)
                {
                    break;
                }
                minXMove--;
            }
            int maxXMove = 0;
            bool rightEnd = false;
            for (int x = end.X; x < TempLand.GetLength(0); x++)
            {
                for (int y = start.Y; y < start.Y + height; y++)
                {
                    if (TempLand[x, y] != 0)
                    {
                        rightEnd = true;
                        break;
                    }
                }
                if (rightEnd)
                {
                    break;
                }
                maxXMove++;
            }
            if (minXMove < -4)
            {
                minXMove += 4;
            }
            if (maxXMove > 4)
            {
                maxXMove -= 4;
            }

            //Moving the shape on the X axis.
            int endXMove = Seed.seed.Next(minXMove, maxXMove);
            shapes[i].MoveBy(new Dot(endXMove, 0));

            //Updating collision data.
            WriteShapeToTempLand(shapes[i]);

            //Starting an inline (first come, first served) task to create finalized shape contour on a separate thread,
            //while the rest of the shapes are being moved.
            int ind = i;
            tasks.Add(factory.StartNew(() => CreateShapeContour(shapes[ind])));
        }

        //Waiting for inline tasks to finish.
        Task[] clearTasks = tasks.ToArray();
        Task.WaitAll(clearTasks);

        //Creating a new task array to fill each shape individually on any available thread.
        clearTasks = new Task[shapes.Count];
        for (int i = 0; i < shapes.Count; i++)
        {
            //Creating a random dot for noise sampling.
            Dot randStartDot = new Dot(Seed.seed.Next(0, 1024), Seed.seed.Next(0, 1024));
            int ind = i;
            clearTasks[i] = Task.Factory.StartNew(() => { WriteShapeToHeightMap(shapes[ind],randStartDot); });
        }
        Task.WaitAll(clearTasks);
        return PreMap;
    }

    //Generates all the data needed for shape creation, then starts it.
    static Shape AddShape(int id)
    {
        Box shapeBox = GridController.GetNextBox();
        Shape shape;
        //Checking if there is any space available for shape creation.
        if (shapeBox != null)
        {
            //Calculating randomized fill (end shape size) values for each individual shape.
            float fillTarget = Mathf.Lerp(Options.MinFill*100, Options.MaxFill * 100, Options.ShapeSize);
            float targetThreshold = fillTarget / 10;
            fillTarget = Seed.seed.Next((int)(fillTarget - targetThreshold), (int)(fillTarget + targetThreshold));
            fillTarget /= 100f;
            //Determining end shape rotation.
            float angle = Seed.seed.Next(-Options.MaxShapeRotationAngle, Options.MaxShapeRotationAngle);
            shape = new Shape(id, shapeBox, fillTarget, angle);
            //Writing collision info.
            WriteShapeToTempLand(shape);
            return shape;
        }
        else
        {
            return null;
        }
    }

    //Separate method that can be called from other threads.
    static void CreateShapeContour(Shape shape)
    {
        shape.CreateCountour(DenistyController);
    }

    //Separate method that can be called from other threads. Fills the shape with noise data, then writes it to the finished map.
    static void WriteShapeToHeightMap(Shape shape, Dot startDot)
    {
        shape.FillShapeWithLand(startDot, null);
        for (int x = 0; x < shape.HeightMap.GetLength(0); x++)
        {
            for (int y = 0; y < shape.HeightMap.GetLength(1); y++)
            {
                if (shape.HeightMap[x, y] != 0)
                {
                    PreMap[x + shape.OuterBox.Start.X, y + shape.OuterBox.Start.Y] = shape.HeightMap[x, y];
                }
            }
        }
    }

    //Writes given shape to the collision map for further shape movements.
    static void WriteShapeToTempLand(Shape shape)
    {
        for (int y = 0; y < shape.OuterBox.TempLand.GetLength(1); y++)
        {
            for (int x = 0; x < shape.OuterBox.TempLand.GetLength(0); x++)
            {
                if (shape.OuterBox.TempLand[x, y] != 0)
                {
                    Dot globalDot = new Dot(x + shape.OuterBox.Start.X, y + shape.OuterBox.Start.Y);
                    TempLand[globalDot.X, globalDot.Y] = shape.Id;
                }
            }
        }
    }

    //Removes given shape from the collision map before moving it.
    static void RemoveShapeFromTempLand(Shape shape)
    {
        for (int y = 0; y < shape.OuterBox.TempLand.GetLength(1); y++)
        {
            for (int x = 0; x < shape.OuterBox.TempLand.GetLength(0); x++)
            {
                Dot globalDot = new Dot(x + shape.OuterBox.Start.X, y + shape.OuterBox.Start.Y);
                if (TempLand[globalDot.X, globalDot.Y] == shape.Id)
                {
                    TempLand[globalDot.X, globalDot.Y] = 0;
                }
            }
        }
    }
}
