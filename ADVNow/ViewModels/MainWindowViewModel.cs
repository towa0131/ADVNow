using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Extensions;
using ADVNow.Models;
using ADVNow.Commands;
using System.IO;
using NovelGameLib.Database;
using NovelGameLib;
using NovelGameLib.Entity;
using System.Linq;
using SqlKata;
using SqlKata.Execution;
using SqlKata.Compilers;
using System.Data.SQLite;

namespace ADVNow.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public ReactiveCollection<Game> AllGames { get; set; } = new ReactiveCollection<Game>();

        public ReactiveCollection<Game> Games { get; set; } = new ReactiveCollection<Game>();

        public ICommand ClickCommand { get; set; }

        public ICommand UpdateGameListCmd { get; set; }

        public ICommand ExitCmd { get; set; }

        public ICommand SetBackgroundCmd { get; set; }

        public ICommand AddGameCmd { get; set; }

        public NovelGameAPI API { get; set; }

        public UserData UserData;

        public ReactiveProperty<string> BackgroundImage { get; set; }

        public ReactiveProperty<bool> DiscordStatus { get; set; }

        public ReactiveCollection<string> BrandList { get; set; } = new ReactiveCollection<string>();

        public ReactiveCollection<int> YearList { get; set; } = new ReactiveCollection<int>();

        public ReactiveCollection<string> ShowList { get; set; } = new ReactiveCollection<string>();

        public ReactiveProperty<int> ShowType { get; set; } = new ReactiveProperty<int>();

        public ReactiveProperty<int> SelectedList { get; set; } = new ReactiveProperty<int>();

        private QueryFactory db;

        public MainWindowViewModel()
        {
            string documentFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ADVNow";
            if (!Directory.Exists(documentFolder))
            {
                Directory.CreateDirectory(documentFolder);
            }
            string gameDBFile = documentFolder + "\\games.db";
            if (File.Exists(gameDBFile))
            {
                this.API = new NovelGameAPI(new SQLiteDatabase(gameDBFile));
            }
            else
            {
                this.API = new NovelGameAPI(new ErogameScapeDatabase());
                Task downloadTask = new Task(async () =>
                {
                    IExportableDatabase db = new ErogameScapeDatabase();
                    await db.ExportToSQLite3(gameDBFile);
                    GC.Collect();
                    this.API = new NovelGameAPI(new SQLiteDatabase(gameDBFile));
                });
                downloadTask.Start();
            }

            // UserData
            string userDBFile = documentFolder + "\\user.db";
            var connectionstring = new SQLiteConnectionStringBuilder
            {
                DataSource = userDBFile
            };

            SQLiteConnection connection = new SQLiteConnection(connectionstring.ToString());
            SqliteCompiler compiler = new SqliteCompiler();

            connection.Open();

            this.db = new QueryFactory(connection, compiler);

            SQLiteCommand command = new SQLiteCommand(connection);

            command.CommandText = "CREATE TABLE IF NOT EXISTS user(" +
                "Background TEXT, " +
                "DiscordStatus BOOL)";
            if (command.ExecuteNonQuery() != -1)
            {
                this.UserData = new UserData()
                {
                    Background = "",
                    DiscordStatus = true
                };
                this.db.Query("user").Insert(this.UserData);
            }
            else
            {
                this.UserData = this.db.Query("user").First<UserData>();
            }

            this.BackgroundImage = ReactiveProperty.FromObject(this.UserData, x => x.Background);
            this.DiscordStatus = ReactiveProperty.FromObject(this.UserData, x => x.DiscordStatus);
            this.BackgroundImage.Subscribe((x) => this.db.Query("user").Update(this.UserData));
            this.DiscordStatus.Subscribe((x) => this.db.Query("user").Update(this.UserData));

            this.ShowType.Value = 0;
            this.SelectedList.Value = 0;

            // デバッグ用
            this.ClickCommand = new DelegateCommand(async () =>
            {
                List<NovelGame> games = await this.API.SearchGames("妹");
                foreach (NovelGame game in games)
                {
                    Brand brand = await this.API.SearchBrandById(game.BrandId.Value);
                    Game g = new Game()
                    {
                        Title = game?.Title ?? "",
                        Brand = brand?.Name ?? "",
                        SellDay = game.SellDay?.ToString("yyyy年MM月"),
                        LastPlay = DateTime.Now.ToString("yyyy年MM月dd日"),
                        TotalPlayMinutes = 0
                    };
                    this.AllGames.Add(g);
                    if (brand == null)
                    {
                        this.ShowList.Add("不明");
                    } else if (!this.ShowList.Contains(brand.Name))
                    {
                        this.ShowList.Add(brand.Name);
                    }
                }
            }, canExcuteCommand);

            // Commands
            this.UpdateGameListCmd = new UpdateGameListCommand(this);
            this.ExitCmd = new ExitCommand();
            this.SetBackgroundCmd = new SetBackgroundCommand(this);
            this.AddGameCmd = new AddGameCommand(this);

            this.AllGames.ObserveAddChanged().Subscribe((game) =>
            {
                if (!this.BrandList.Contains(game.Brand))
                {
                    this.BrandList.Add(game.Brand);
                }
                int year = Convert.ToInt32(DateTime.Parse(game.SellDay).ToString("yyyy"));
                if (!this.YearList.Contains(year))
                {
                    this.YearList.Add(year);
                }
                this.UpdateGameListCmd.Execute(new List<Game> { game });
            });

            this.AllGames.ObserveRemoveChanged().Subscribe((game) => 
            {
                bool flagBrand = true;
                bool flagYear = true;
                int year = Convert.ToInt32(DateTime.Parse(game.SellDay).ToString("yyyy"));
                foreach (Game g in this.AllGames)
                {
                    int gYear = Convert.ToInt32(DateTime.Parse(g.SellDay).ToString("yyyy"));
                    if (g.Brand == game.Brand) flagBrand = false;
                    if (year == gYear) flagYear = false;
                }
                if (flagBrand) this.BrandList.Remove(game.Brand);
                if (flagYear) this.YearList.Remove(year);
                if (this.Games.Contains(game))
                {
                    this.Games.Remove(game);
                }
            });

            this.ShowType.Subscribe((t) => {
                ShowList.Clear();
                this.SelectedList.Value = 0;
                this.ShowList.Add("全て");
                switch (t)
                {
                    case 0:
                        foreach (string brand in this.BrandList)
                        {
                            ShowList.Add(brand);
                        }
                        break;
                    case 1:
                        foreach (int year in this.YearList)
                        {
                            ShowList.Add(year.ToString() + "年");
                        }
                        break;
                }
                this.Games.Clear();
                this.UpdateGameListCmd.Execute(this.AllGames.ToList());
            });

            this.SelectedList.Subscribe((i) =>
            {
                this.Games.Clear();
                this.UpdateGameListCmd.Execute(this.AllGames.ToList());
            });
        }

        public bool canExcuteCommand()
        {
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
