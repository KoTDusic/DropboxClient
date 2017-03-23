using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dropbox.Api.Files;
using DropboxTest;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls.Primitives;

namespace PDWDropboxClient
{
    enum DataOperations {copy,cut,none};
    
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PDWDropbox client;
        string currentDir;
        string PathFrom;
        DataOperations currentOperation;
        BackgroundWorker directoryWorker;
        BackgroundWorker directoryCreaterWorker;
        BackgroundWorker directoryDeleteWorker;
        BackgroundWorker UserinfoGetterWorker;
        BackgroundWorker ElementCopyWorker;
        BackgroundWorker ElementCutWorker;
        BackgroundWorker FilesUploadWorker;
        BackgroundWorker FilesDownloadWorker;
        BackgroundWorker ElementSharingWorker;
        BackgroundWorker SearchWorker;
        BackgroundWorker SharedListLoadWorker;
        StackPanel selectedPanel;
        Dictionary<string,ItemInformation> item_array;
        ContextMenu itemRightclickMenu;
        private MenuItem CreateContexMenuItem(string name, string imagepath)
        {
            MenuItem item = new MenuItem();
            item.Header = name;
            Image del_icon = new Image();
            del_icon.Source = GetImageFromURI(imagepath);
            del_icon.Width = 20;
            del_icon.Height = 20;
            item.Icon = del_icon;
            item.Click += Item_Click;
            return item;
        }
        private void ChangeCurrentOperation(DataOperations newState)
        {
            MenuItem item = findMenuItemByName("Вставить");
            switch(newState)
            {
                case DataOperations.none:
                    currentOperation = DataOperations.none;
                    item.IsEnabled = false;
                    break;
                case DataOperations.copy:
                    currentOperation = DataOperations.copy;
                    item.IsEnabled = true;
                    break;
                case DataOperations.cut:
                    currentOperation = DataOperations.cut;
                    item.IsEnabled = true;
                    break;
            }
        }
        private MenuItem findMenuItemByName(string name)
        {
            string buttonName;
            for(int i=0;i<itemRightclickMenu.Items.Count;i++)
            {
                buttonName=(string)(((MenuItem)itemRightclickMenu.Items[i]).Header);
                if (buttonName == name) return (MenuItem)itemRightclickMenu.Items[i];
            }
            for(int i=0;i<itemRightclickMenu.Items.Count;i++)
            {
                buttonName = (string)(((MenuItem)RightclickMenu.Items[i]).Header);
                if (buttonName == name) return (MenuItem)RightclickMenu.Items[i];
            }
            
            return null;
        }
        private void CreateContextMenu()
        {
            itemRightclickMenu = new ContextMenu();
            itemRightclickMenu.Items.Add(CreateContexMenuItem("Удалить", "Resources/del_icon.png"));
            itemRightclickMenu.Items.Add(CreateContexMenuItem("Копировать", "Resources/copy_icon.png"));
            itemRightclickMenu.Items.Add(CreateContexMenuItem("Вырезать", "Resources/cut_icon.png"));
            itemRightclickMenu.Items.Add(CreateContexMenuItem("Расшарить", "Resources/share_icon.png"));
            ChangeCurrentOperation(DataOperations.none);
        }
        public MainWindow()
        {
            InitializeComponent();
            selectedPanel = null;
            client = new PDWDropbox();
            CreateContextMenu();
            directoryWorker = new BackgroundWorker();
            item_array = new Dictionary<string, ItemInformation>();
            #region WorkersInit
            directoryWorker.DoWork += directoryWorker_DoWork;
            directoryWorker.RunWorkerCompleted += directoryWorker_RunWorkerCompleted;
            LoadCatalogAsunc("");

            directoryCreaterWorker = new BackgroundWorker();
            directoryCreaterWorker.DoWork += directoryCreaterWorker_DoWork;
            directoryCreaterWorker.RunWorkerCompleted += directoryCreaterWorker_RunWorkerCompleted;

            directoryDeleteWorker = new BackgroundWorker();
            directoryDeleteWorker.DoWork += DirectoryDeleteWorker_DoWork; ;
            directoryDeleteWorker.RunWorkerCompleted += DirectoryDeleteWorker_RunWorkerCompleted;

            UserinfoGetterWorker = new BackgroundWorker();
            UserinfoGetterWorker.DoWork+= UserinfoGetterWorker_DoWork;
            UserinfoGetterWorker.RunWorkerCompleted += UserinfoGetterWorker_RunWorkerCompleted;

            ElementCopyWorker = new BackgroundWorker();
            ElementCopyWorker.DoWork += ElementCopyWorker_DoWork;
            ElementCopyWorker.RunWorkerCompleted += ElementCopyWorker_RunWorkerCompleted;

            ElementCutWorker = new BackgroundWorker();
            ElementCutWorker.DoWork += ElementCutWorker_DoWork;
            ElementCutWorker.RunWorkerCompleted += ElementCutWorker_RunWorkerCompleted;

            FilesUploadWorker = new BackgroundWorker();
            FilesUploadWorker.WorkerReportsProgress = true;
            FilesUploadWorker.DoWork += FilesUploadWorker_DoWork;
            FilesUploadWorker.RunWorkerCompleted += FilesUploadWorker_RunWorkerCompleted;
            FilesUploadWorker.ProgressChanged += FilesUploadWorker_ProgressChanged;

            FilesDownloadWorker = new BackgroundWorker();
            FilesDownloadWorker.DoWork += FilesDownloadWorker_DoWork;
            FilesDownloadWorker.RunWorkerCompleted += FilesDownloadWorker_RunWorkerCompleted;


            ElementSharingWorker = new BackgroundWorker();
            ElementSharingWorker.DoWork += ElementSharingWorker_DoWork;
            ElementSharingWorker.RunWorkerCompleted += ElementSharingWorker_RunWorkerCompleted;

            SearchWorker = new BackgroundWorker();
            SearchWorker.DoWork+=SearchWorker_DoWork;
            SearchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;

            SharedListLoadWorker = new BackgroundWorker();
            SharedListLoadWorker.DoWork+=SharedListLoadWorker_DoWork;
            SharedListLoadWorker.RunWorkerCompleted += SharedListLoadWorker_RunWorkerCompleted;
            #endregion
        }
#region Workers
        void SharedListLoadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var task = client.ListOfSharedLinksAsync();
            e.Result = task.Result;
        }
        void SharedListLoadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ItemInformation[] array = (ItemInformation[])e.Result;
            string result_string="тип объекта".PadRight(12)+ "|" +"путь на диске".PadLeft(23).PadRight(31)  +"|"+"ссылка".PadLeft(43).PadRight(85)+"|\n";
            result_string += "".PadRight(106, '-') + "\n";
            string cut_name = "";
            
            string obj_type="";
            for(int i=0;i<array.Length;i++)
            {
                if (array[i].Name.Length >= 20) cut_name = array[i].Name.Substring(0, 20) + "...";
                else cut_name = array[i].Name;
                if (array[i].Type == ElementType.dir) obj_type = "папка";
                else obj_type = "файл";
                result_string += obj_type.PadRight(15) + "  |  " + cut_name.PadRight(30) + "  |  " + array[i].Share_url + "\n";
                result_string +="".PadRight(106,'-') +"\n";
            }
            MyInputDialog dialog = new MyInputDialog("Список расшареных объектов", false, result_string, 1000, 700);
            dialog.ShowDialog();
        }
        void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string serchString = (string)e.Argument;
            var task = client.SearchAsync(currentDir, serchString);
            task.Wait();
            e.Result = task.Result;
        }
        void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ItemInformation[] res = (ItemInformation[])e.Result;
            Contents.Children.Clear();
            item_array.Clear();
            for (int i = 0; i < res.Length; i++)
            {
                Contents.Children.Add(CreatePanelItem(res[i].Name, res[i].Type));
                item_array.Add(res[i].Name, res[i]);
            }
        }
        void ElementSharingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = (string)e.Argument;
            var task = client.ShareAsync(path);
            task.Wait();
            e.Result = task.Result;
        }
        void ElementSharingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ItemInformation sharedItem = (ItemInformation)e.Result;
            MyInputDialog dialog = new MyInputDialog("Ссылка на " + sharedItem.Name, false, sharedItem.Share_url);
            dialog.ShowDialog();
        }
        void FilesDownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = ((string[])e.Argument)[0];
            string save_dir = ((string[])e.Argument)[1];
            string[] temp_array=path.Split('\\');
            System.Windows.Forms.SaveFileDialog dialog=new System.Windows.Forms.SaveFileDialog();
            dialog.FileName=temp_array[temp_array.Length-1];
            var task = client.DownloadAsync(path, save_dir);
            e.Result = temp_array[temp_array.Length - 1];
        }   
        void FilesDownloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("файл " + e.Result + " загружен");
        }
        void FilesUploadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] files = (string[])e.Argument;
            string[] temp_arr;
            FileStream stream;
            Task<ItemInformation> task;
            string path = currentDir;
            ItemInformation[] results=new ItemInformation[files.Length];
            for(int i=0;i<files.Length;i++)
            {
                stream = File.Open(files[i],FileMode.Open);
                temp_arr = files[i].Split('\\');
                task = client.UploadAsync(path + "/" + temp_arr[temp_arr.Length - 1], stream);
                try
                {
                    task.Wait();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.InnerException.Message);
                }
                FilesUploadWorker.ReportProgress(0);
                results[i] = task.Result;
            }
            e.Result = results;
        }
        void FilesUploadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value++;
        }
        void FilesUploadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ItemInformation[] results = (ItemInformation[])e.Result;
            ItemInformation info;
            for(int i = 0;i<results.Length;i++)
            {
                if (!item_array.TryGetValue(results[i].Name, out info))
                {
                    Contents.Children.Add(CreatePanelItem(results[i].Name, results[i].Type));
                    item_array.Add(results[i].Name, results[i]); 
                }
            }
            
        }
        private void ElementCopyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] mas = (string[])e.Argument;
            string pathTo = mas[0];
            string pathFrom = mas[1];

            var task = client.CopyAsync(pathFrom, pathTo);
            task.Wait();
            e.Result = task.Result;
        }
        private void ElementCopyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChangeCurrentOperation(DataOperations.none);
            ItemInformation res = (ItemInformation)e.Result;
            Contents.Children.Add(CreatePanelItem(res.Name, res.Type));
            item_array.Add(res.Name, res);
        }
        private void ElementCutWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] mas = (string[])e.Argument;
            string pathTo = mas[0];
            string pathFrom = mas[1];
            var task = client.MoveAsync(pathFrom, pathTo);
            task.Wait();
            e.Result = task.Result;
        }
        private void ElementCutWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChangeCurrentOperation(DataOperations.none);
            ItemInformation res = (ItemInformation)e.Result;
            Contents.Children.Add(CreatePanelItem(res.Name, res.Type));
            item_array.Add(res.Name, res);
        }
        private void directoryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var task = client.GetFilesList((string)e.Argument);
            task.Wait();
            e.Result = task.Result;
        }
        private void directoryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DownloadIndicator.Visibility = System.Windows.Visibility.Collapsed;

            Contents.Visibility = System.Windows.Visibility.Visible;
            Contents.Children.Clear();
            item_array.Clear();
            ItemInformation[] res = (ItemInformation[])e.Result;
            if (currentDir != "")
            {
                Contents.Children.Add(CreatePanelItem("...", ElementType.back));
            }
            for (int i = 0; i < res.Length; i++)
            {
                Contents.Children.Add(CreatePanelItem(res[i].Name, res[i].Type));
                item_array.Add(res[i].Name, res[i]);
            }
            RightclickMenu.Visibility = Visibility.Visible;
        }
        void directoryCreaterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var task = client.CreateDirectory((string)e.Argument);
            task.Wait();
            e.Result = task.Result;
        }
        void directoryCreaterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ItemInformation info;
            ItemInformation item = (ItemInformation)e.Result;
            if (!item_array.TryGetValue(item.Name, out info))
            {
                item_array.Add(item.Name, item);
                Contents.Children.Insert(1, CreatePanelItem(item.Name, item.Type));
            }
        }
        private void DirectoryDeleteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var task = client.DeleteAsync((string)e.Argument);
            task.Wait();
            e.Result = task.Result;
        }
        private void DirectoryDeleteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string name = ((Metadata)e.Result).Name;
            LoadCatalogAsunc(currentDir);
        }
        private void UserinfoGetterWorker_DoWork(object sender, DoWorkEventArgs e) 
        {
            var task = client.GetAccountInfo();
            var task2 = client.GetSpaceInfo();
            task.Wait();
            task2.Wait();
            UserInfo result=task.Result;
            result.spaceInfo=task2.Result;
            e.Result = result;
        }
        private void UserinfoGetterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UserInfoPage page = new UserInfoPage((UserInfo)e.Result);
            page.ShowDialog();
        }
        #endregion
#region AsuncFunctions
        void LoadCatalogAsunc(string path)
        {
            RightclickMenu.Visibility = Visibility.Collapsed;
            currentDir = path;
            if (path == "")
            {
                DisplayedDir.Content = "корневой каталог";
            }
            else
            {
                DisplayedDir.Content = path;
            }
            Contents.Visibility = Visibility.Hidden;
            DownloadIndicator.Visibility = Visibility.Visible;
            directoryWorker.RunWorkerAsync(currentDir);
        }
        void CreateFolderAsunc()
        {
            MyInputDialog dialog = new MyInputDialog("Введите имя папки", editable: true);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();
            if (!dialog.cancelled && dialog.folderName.Length > 0)
            {
                directoryCreaterWorker.RunWorkerAsync(currentDir + "/" + dialog.folderName);
            }
        }
        void DeleteElementAsunc(string path, string itemname)
        {
            MyInputDialog dialog = new MyInputDialog("Действительно удалить " + itemname + "?", editable: false, inputText: path);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();
            if (!dialog.cancelled)
            {
                directoryDeleteWorker.RunWorkerAsync(path);
            }
        }
        void OpenUserInfoAsunc()
        {
            UserinfoGetterWorker.RunWorkerAsync();
        }
        void CopyElementAsunc(string From, string To)
        {
            ElementCopyWorker.RunWorkerAsync(new string[] { To, From });
        }
        void CutElementAsunc(string From, string To)
        {
            ElementCutWorker.RunWorkerAsync(new string[] { To, From });
        }
        void UploadFilesAsunc(string[] files)
        {
            FilesUploadWorker.RunWorkerAsync(files);
        }
        void DownloadFileAsunc(string file, string save_dir)
        {
            FilesDownloadWorker.RunWorkerAsync(new string[] { file, save_dir });
        }
        void ShareElementAsunc(string path)
        {
            ElementSharingWorker.RunWorkerAsync(path);
        }
        void SearchAsunc(string querry)
        {
            SearchWorker.RunWorkerAsync(querry);
        }
        void SharedListAsunc()
        {
            SharedListLoadWorker.RunWorkerAsync();
        }
#endregion
        private void Item_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem=(MenuItem)sender;
            var menu = (ContextMenu)(menuItem).Parent;
            var element = (StackPanel)menu.PlacementTarget;
            string item_name = (string)((Label)element.Children[1]).Content;
            ItemInformation item = item_array[item_name];
            switch((string)menuItem.Header)
            {
                case "Удалить":
                    string path = item_array[item_name].Fullpath;
                    DeleteElementAsunc(path, item_name);
                    break;
                case "Копировать":
                    ChangeCurrentOperation(DataOperations.copy);
                    PathFrom = item.Fullpath;
                    break;
                case "Вырезать":
                    ChangeCurrentOperation(DataOperations.cut);
                    PathFrom = item.Fullpath;
                    break;
                case "Расшарить":
                    ShareElementAsunc(item.Fullpath);
                    break;
            }
            

        }
        StackPanel CreatePanelItem(string text,ElementType type)
        {
            StackPanel panel = new StackPanel();
            Image img = new Image();
            Label label = new Label();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (type != ElementType.back) panel.ContextMenu = itemRightclickMenu;
            panel.Width = Double.NaN;
            panel.MouseLeftButtonDown += panel_MouseLeftButtonDown;
            panel.MouseRightButtonDown += panel_MouseRightButtonDown;
            label.Content = text;
            label.FontSize = 20;
            label.HorizontalAlignment = HorizontalAlignment.Stretch;
            label.Width = Double.NaN;
            img.Width = 20;
            img.Height = 20;
            switch (type)
            {
                case ElementType.dir:
                    img.Source = GetImageFromURI("Resources/dir_icon.png");
                    break;
                case ElementType.pdf:
                    img.Source = GetImageFromURI("Resources/pdf_icon.png");
                    break;
                case ElementType.exe:
                    img.Source = GetImageFromURI("Resources/exe_icon.png");
                    break;
                case ElementType.img:
                    img.Source = GetImageFromURI("Resources/img_icon.png");
                    break;
                case ElementType.back:
                    img.Source = GetImageFromURI("Resources/back_icon.png");
                    break;
                default:
                    img.Source = GetImageFromURI("Resources/folder.png");
                    break;
            }
            panel.Children.Add(img);
            panel.Children.Add(label);
            return panel;
        }
        void panel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                StackPanel panel = (StackPanel)sender;
                Label label = (Label)panel.Children[1];
                if (selectedPanel != null) selectedPanel.Background = null;
                selectedPanel = panel;
                panel.Background = new SolidColorBrush(Colors.LightBlue);
            }
        }
        void panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel panel = (StackPanel)sender;
            Label label = (Label)panel.Children[1];
            if(e.ClickCount == 1)
            {
                if (selectedPanel != null) selectedPanel.Background = null;
                selectedPanel = panel;
                panel.Background = new SolidColorBrush(Colors.LightBlue);
            }
            if (e.ClickCount == 2)
            {
                
                ItemInformation element;
                if (label.Content.ToString() == "...")
                {
                    string[] pathParts = currentDir.Split(new char[]{'/'},StringSplitOptions.RemoveEmptyEntries);
                    currentDir = "";
                    for (int i = 0; i < pathParts.Length - 1; i++)
                    {
                        currentDir +="/" + pathParts[i];
                    }
                    LoadCatalogAsunc(currentDir);
                }
                else
                {
                    if (item_array.TryGetValue((string)label.Content, out element))
                    {
                        if (element.Type == ElementType.dir)
                        {
                            LoadCatalogAsunc(element.Fullpath);
                        }
                        else
                        {
                            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
                            dialog.FileName = element.Name;
                            if(dialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                            {
                                DownloadFileAsunc(element.Fullpath, dialog.FileName);
                            }
                            
                        }
                    }
                }
            } 
        }
        private BitmapImage GetImageFromURI(string uri)
        {
            return new BitmapImage(new Uri(uri, UriKind.Relative));
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch((string)((MenuItem)sender).Header)
            {
                case "Создать папку...":
                    {
                        CreateFolderAsunc();
                    }
                    break;
                case "Вставить":
                    {
                        string[] parts = PathFrom.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        string element_name = parts[parts.Length - 1];
                        switch(currentOperation)
                        {
                            case DataOperations.cut:
                                CutElementAsunc(PathFrom, currentDir + "/" + element_name);
                                break;
                            case DataOperations.copy:
                                CopyElementAsunc(PathFrom, currentDir + "/" + element_name);
                                break;
                        }
                        ChangeCurrentOperation(DataOperations.none);
                    }
                    break;
            }
            
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenUserInfoAsunc();
        }
        private void ScrollViewer_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string files_string="";
            foreach(string str in files)
            {
                files_string+=str+"\n";
            }
            string question = "";
            if(currentDir!="")
            {
                question = "Загрузить эти файлы в " + currentDir + "?";
            }
            else question="Загрузить эти файлы в корень?";
            MyInputDialog dialog = new MyInputDialog(question, false, files_string,1000,700);
            dialog.ShowDialog();
            if(!dialog.cancelled)
            {
                progressBar.Maximum = files.Length;
                progressBar.Value = 0;
                UploadFilesAsunc(files);
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MyInputDialog dialog = new MyInputDialog("Введите поисковый запрос", true);
            dialog.ShowDialog();
            if(!dialog.cancelled)
            {
                SearchAsunc(dialog.folderName);
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            LoadCatalogAsunc("");
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            SharedListAsunc();
        }
    }
}
