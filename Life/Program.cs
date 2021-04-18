using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Newtonsoft.Json;
using System.IO;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public bool IsChecked = false;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
        public void Check()
        {
            IsChecked = true;
            foreach (var neighbour in neighbors)
            {
                if (neighbour.IsAlive == true && neighbour.IsChecked == false)
                {
                    neighbour.Check();
                }              
            }
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        [JsonConstructor]public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }
        public Board(string inputText) 
        {
            CellSize = 1;

            string[] text = inputText.Split('\n');
            int textColumns = text[0].Length;
            int textRows = 1 +(inputText.Length - textColumns) / textColumns;
            Cells = new Cell[textColumns, textRows];

            Console.WriteLine(Cells.GetLength(0));
            Console.WriteLine(Cells.GetLength(1));
            for(int i = 0; i < textRows; i ++)
            {
                for(int j = 0; j < textColumns; j++)
                {
                    Cells[j,i] = new Cell();
                    if (text[i][j] == '*')
                    {
                        Cells[j,i].IsAlive = true;
                    }
                    else
                    {
                        Cells[j,i].IsAlive = false;
                    }
                }
            }
            ConnectNeighbors();
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
        public void Stats()
        {
            int counterFigure = 0;
            int counterCells = 0;
            for(int i = 0; i < Rows; i ++)
            {
                for(int j = 0; j < Columns; j ++)
                {
                    if(Cells[j,i].IsAlive == true)
                        {
                            counterCells += 1;
                            if(Cells[j,i].IsChecked ==  false)
                                {
                                    Console.WriteLine("Figure");
                                    counterFigure += 1;
                                    Cells[j,i].Check();
                                }
                        }
                    
                }
            }
            Console.WriteLine("Cells "+counterCells);
            Console.WriteLine("Figures "+counterFigure);
        }
        public void Save(string name)
        {
            string text = "";
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)   
                {
                    var cell = Cells[col, row];
                    if (cell.IsAlive)
                    {
                        text += '*';
                    }
                    else
                    {
                        text += ' ';
                    }
                }
                text += '\n';
            }
            File.WriteAllText(".\\"+name+".txt", text);

        }

        
    }
    


    class Program
    {
        static bool Flag = true;
        static Board board;
        static private void Reset(string JsonPath = "")
        {
            if (JsonPath == "")
            {   
                board = new Board(
                width: 50,
                height: 20,
                cellSize: 1,
                liveDensity: 0.5);
            }
            else
            {               
                Console.WriteLine(File.ReadAllText(JsonPath)) ;
                Dictionary<string, double> dict = JsonSerializer.Deserialize<Dictionary<string, double>>(File.ReadAllText(JsonPath));
                board = new Board((int)dict["width"],(int)dict["height"],(int)dict["cellSize"],dict["liveDensity"]);                
            }
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)   
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("Do you want to read from file? y/n");
            string x = Console.ReadLine();
            if(x == "y")
            {
                Console.WriteLine("Enter File name");
                string Name = Console.ReadLine();
                Read(Name);
            }
            else
            {
                Reset("board.json"); //put your JSON file here
            }
                       
            Thread A = new Thread(WaitButton);
            A.Start();
            while(Flag)
            {
                Console.Clear();
                Render();
                board.Advance();                
                Thread.Sleep(100);
            }
            board.Stats();
            Console.WriteLine("Do you want to save? y/n");
            string y = Console.ReadLine();
            if(y == "y")
            {
                Console.WriteLine("Enter File name");
                string Name = Console.ReadLine();
                board.Save(Name);
            }
            Thread.Sleep(10000);

        }
        static void WaitButton()
        {
            Console.ReadKey();
            Flag = false;
        }
        static void Read(string Name)
        {
            string text = File.ReadAllText(Name);
            board = new Board(text);
        }
    }
}