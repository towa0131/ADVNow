using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reflection;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using ADVNow.Models;
using ADVNow.Commands;
using System.IO;
using NovelGameLib.Database;
using NovelGameLib;
using System.Linq;
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

        public ICommand SettingGameCmd { get; set; }

        public ICommand LaunchGameCmd { get; set; }

        public ICommand RemoveGameCmd { get; set; }

        public ICommand MoveErogameScapeCmd { get; set; }

        public ICommand ShareGameCmd { get; set; }

        public NovelGameAPI API { get; set; }

        public UserData UserData;

        public ReactiveProperty<string> BackgroundImage { get; set; }

        public ReactiveProperty<bool> DiscordStatus { get; set; }

        public ReactiveProperty<bool> ShareWithImage { get; set; }

        public ReactiveCollection<string> BrandList { get; set; } = new ReactiveCollection<string>();

        public ReactiveCollection<int> YearList { get; set; } = new ReactiveCollection<int>();

        public ReactiveCollection<string> ShowList { get; set; } = new ReactiveCollection<string>();

        public ReactiveProperty<int> ShowType { get; set; } = new ReactiveProperty<int>();

        public ReactiveProperty<int> SelectedList { get; set; } = new ReactiveProperty<int>();

        public ReactiveProperty<Game> CurrentGame { get; set;} = new ReactiveProperty<Game>();

        public ReactiveProperty<string> PlayingGameString { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> PlayingTimeString { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> SearchGameString { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> VersionString { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> ShareButtonVisibility { get; set; } = new ReactiveProperty<string>();

        public int PlayingGameProcessId;

        public Action onClose;

        private QueryFactory db;

        public MainWindowViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(this.Games, new object());
            BindingOperations.EnableCollectionSynchronization(this.ShowList, new object());
            string documentFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ADVNow";
            if (!Directory.Exists(documentFolder))
            {
                Directory.CreateDirectory(documentFolder);
            }
            string gameDBFile = documentFolder + "\\games.db";
            string gameDBLockFile = documentFolder + "\\games.lock";
            if (File.Exists(gameDBFile) && !File.Exists(gameDBLockFile))
            {
                this.API = new NovelGameAPI(new SQLiteDatabase(gameDBFile));
            }
            else
            {
                this.API = new NovelGameAPI(new ErogameScapeDatabase());
                Task downloadTask = new Task(async () =>
                {
                    FileStream fs = File.Create(gameDBLockFile);
                    fs.Close();
                    IExportableDatabase db = new ErogameScapeDatabase();
                    await db.ExportToSQLite3(gameDBFile);
                    this.API = new NovelGameAPI(new SQLiteDatabase(gameDBFile));
                    File.Delete(gameDBLockFile);
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

            // Load User Data
            command.CommandText = "CREATE TABLE IF NOT EXISTS user(" +
                "Background TEXT, " +
                "DiscordStatus BOOL, " +
                "ShareWithImage BOOL)";
            if (command.ExecuteNonQuery() != -1)
            {
                this.UserData = new UserData()
                {
                    Background = "",
                    DiscordStatus = true,
                    ShareWithImage = true
                };
                this.db.Query("user").Insert(this.UserData);
            }
            else
            {
                this.UserData = this.db.Query("user").First<UserData>();
            }

            this.BackgroundImage = ReactiveProperty.FromObject(this.UserData, x => x.Background);
            this.DiscordStatus = ReactiveProperty.FromObject(this.UserData, x => x.DiscordStatus);
            this.ShareWithImage = ReactiveProperty.FromObject(this.UserData, x => x.ShareWithImage);
            this.BackgroundImage.Subscribe((x) => this.db.Query("user").Update(this.UserData));
            this.DiscordStatus.Subscribe((x) => this.db.Query("user").Update(this.UserData));
            this.ShareWithImage.Subscribe((x) => this.db.Query("user").Update(this.UserData));

            this.ShowType.Value = 0;
            this.SelectedList.Value = 0;
            this.PlayingGameString.Value = "---";
            this.PlayingTimeString.Value = "";
            this.SearchGameString.Value = "";
            this.ShareButtonVisibility.Value = "Hidden";
            this.VersionString.Value = "ADVNow Version " + Assembly.GetExecutingAssembly().GetName().Version;

            this.PlayingGameProcessId = -1;

            // Commands
            this.UpdateGameListCmd = new UpdateGameListCommand(this);
            this.ExitCmd = new ExitCommand();
            this.SetBackgroundCmd = new SetBackgroundCommand(this);
            this.AddGameCmd = new AddGameCommand(this);
            this.LaunchGameCmd = new LaunchGameCommand(this, "770721176355078155", "f907931f3e0c24d");
            this.RemoveGameCmd = new RemoveGameCommand(this);
            this.MoveErogameScapeCmd = new MoveErogameScapeCommand(this);
            this.ShareGameCmd = new ShareGameCommand(this, "f907931f3e0c24d");
            this.SettingGameCmd = new SettingGameCommand(this);

            // Property Subscribe
            this.AllGames.ObserveAddChanged().Subscribe((game) =>
            {
                if (!this.BrandList.Contains(game.Brand))
                {
                    int index = this.BrandList.Count();
                    for (int i = 0; i < this.BrandList.Count(); i++)
                    {
                        if (String.Compare(this.BrandList[i], game.Brand) > 0)
                        {
                            index = i;
                            break;
                        }
                    }
                    this.BrandList.Insert(index, game.Brand);
                    if (this.ShowType.Value == 0)
                    {
                        this.ShowList.Insert(index + 1, game.Brand);
                    }
                }
                int year = Convert.ToInt32(game.SellDay.ToString("yyyy"));
                if (!this.YearList.Contains(year))
                {
                    int minIndex = this.YearList.Count();
                    for (int i = 0; i < this.YearList.Count(); i++)
                    {
                        if (this.YearList[i] < year)
                        {
                            minIndex = i;
                            break;
                        }
                    }
                    this.YearList.Insert(minIndex, year);
                    if (this.ShowType.Value == 1) this.ShowList.Insert(minIndex + 1, year.ToString() + "年");
                }
                this.UpdateGameListCmd.Execute(new List<Game> { game });
                if (this.db.Query("games").Where("Title", game.Title).Get<Game>().ToList().Count() == 0)
                {
                    this.db.Query("games").Insert(game);
                }
            });

            this.AllGames.ObserveRemoveChanged().Subscribe((game) => 
            {
                bool flagBrand = true;
                bool flagYear = true;
                int year = Convert.ToInt32(game.SellDay.ToString("yyyy"));
                foreach (Game g in this.AllGames)
                {
                    int gYear = Convert.ToInt32(g.SellDay.ToString("yyyy"));
                    if (g.Brand == game.Brand) flagBrand = false;
                    if (year == gYear) flagYear = false;
                }
                if (flagBrand)
                {
                    this.BrandList.Remove(game.Brand);
                    if (this.ShowType.Value == 0)
                    {
                        this.ShowList.Remove(game.Brand);
                        this.SelectedList.Value = 0;
                    }
                }
                if (flagYear)
                {
                    this.YearList.Remove(year);
                    if (this.ShowType.Value == 1)
                    {
                        this.ShowList.Remove(game.SellDay.ToString("yyyy") + "年");
                        this.SelectedList.Value = 0;
                    }
                }
                if (this.Games.Contains(game))
                {
                    this.Games.Remove(game);
                }
                this.db.Query("games").Where("Title", game.Title).Delete();
            });

            this.AllGames.ObserveReplaceChanged().Subscribe((pair) =>
            {
                if (this.Games.ToList().Contains(pair.OldItem)) {
                    this.Games.Remove(pair.OldItem);
                    this.Games.Insert(0, pair.NewItem);
                }
                this.db.Query("games").Where("Title", pair.OldItem.Title).Delete();
                this.db.Query("games").Insert(pair.NewItem);
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
            });

            this.SelectedList.Subscribe((i) =>
            {
                this.Games.Clear();
                this.UpdateGameListCmd.Execute(this.AllGames.ToList());
            });

            this.SearchGameString.Subscribe((t) =>
            {
                this.Games.Clear();
                t = t.ToLower().Replace(" ", "");
                if (t == "")
                {
                    this.UpdateGameListCmd.Execute(this.AllGames.ToList());
                }
                else
                {
                    foreach (Game game in this.AllGames)
                    {
                        if (game.Title.ToLower().Replace(" ", "").Contains(t) ||
                            game.Brand.ToLower().Replace(" ", "").Contains(t))
                        {
                            this.Games.Add(game);
                        }
                    }
                }
            });

            // Load All Games
            command.CommandText = "CREATE TABLE IF NOT EXISTS games(" +
            "Title TEXT, " +
            "Id TEXT, " +
            "Brand TEXT, " +
            "Path TEXT, " +
            "TotalPlayMinutes INTEGER, " +
            "LastPlay DATE, " +
            "SellDay DATE)";
            command.ExecuteNonQuery();
            List<Game> games = this.db.Query("games").OrderByDesc("LastPlay").Get<Game>().ToList();
            foreach (Game game in games)
            {
                this.AllGames.Add(game);
            }
        }

        public bool canExcuteCommand()
        {
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
