namespace GoatLauncher
{
    //Note that .gsb files are really just .zip archives containing backup copies of the %Appdata%\..\Local\Goat2 folder.
    public static class CMDLineProgram
    {
        private static bool ShellExitRequested = false;
        [System.STAThread()]
        public static void CMDLineMain(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                RunCommand(args);
            }
            else
            {
                while (!ShellExitRequested)
                {
                    System.Console.Write("GoatLauncher> ");
                    string command = System.Console.ReadLine();
                    RunCommand(command);
                }
            }
        }
        public static void Write(string message)
        {
            System.Console.Write(message);
        }
        public static void WriteLine()
        {
            System.Console.WriteLine();
        }
        public static void WriteLine(string message)
        {
            System.Console.WriteLine(message);
        }
        public static void WriteError(string message)
        {
            System.ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = System.ConsoleColor.Red;
            System.Console.WriteLine($"Error {message}");
            System.Console.ForegroundColor = originalColor;
        }
        public static void WriteWarning(string message)
        {
            System.ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = System.ConsoleColor.DarkYellow;
            System.Console.WriteLine($"Warning: {message}");
            System.Console.ForegroundColor = originalColor;
        }
        public delegate void BasicTask();
        public static void NoExcept(BasicTask task)
        {
            try
            {
                task.Invoke();
            }
            catch
            {

            }
        }
        public static void RunCmdCommand(string command, string args)
        {
            System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.FileName = command;
            processStartInfo.Arguments = args;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);

            process.WaitForExit();
        }
        public static void RunCommand(string commandString)
        {
            string[] commandLines = commandString.Split(' ');
            RunCommand(commandLines);
        }
        public static void RunCommand(string[] commandLines)
        {
            string command = commandLines[0];
            string[] args = new string[commandLines.Length - 1];
            for (int i = 1; i < commandLines.Length; i++)
            {
                args[i - 1] = commandLines[i];
            }
            RunCommand(command, args);
        }
        public static void RunCommand(string command, string[] args)
        {
            string commandToLower = command.ToLower();
            if (commandToLower is "help")
            {
                HelpCommand(args);
            }
            else if (commandToLower is "save")
            {
                SaveCommand(args);
            }
            else if (commandToLower is "load")
            {
                LoadCommand(args);
            }
            else if (commandToLower is "launch")
            {
                LaunchCommand(args);
            }
            else if (commandToLower is "close")
            {
                CloseCommand(args);
            }
            else if (commandToLower is "clear")
            {
                ClearCommand(args);
            }
            else if (commandToLower is "exit")
            {
                ExitCommand(args);
            }
            else
            {
                WriteError($"Unknown command \"{command}\". Type help for a list of valid commands.");
            }
        }
        //Gives information about how to use GoatLauncher.exe.
        public static void HelpCommand(string[] args)
        {
            if (args is null || args.Length is 0)
            {
                WriteLine();
                WriteLine("Usage: GoatLauncher.exe command args");
                WriteLine("");
                WriteLine("Commands:");
                WriteLine("     Help");
                WriteLine("     Save");
                WriteLine("     Load");
                WriteLine("     Launch");
                WriteLine("     Close");
                WriteLine("     Clear");
                WriteLine("     Exit");
                WriteLine("");
                WriteLine("Use help command for more info on a specific command.");
                WriteLine();
            }
            else if (args.Length != 1)
            {
                WriteError($"Too many arguments in command help. Expected 0 or 1.");
            }
            else
            {
                string command = args[0];
                string commandToLower = command.ToLower();
                if (commandToLower is "help")
                {
                    WriteLine();
                    WriteLine("Displays the help message or help about a specified command.");
                    WriteLine();
                    WriteLine("Usage: help command");
                    WriteLine("Usage: help");
                    WriteLine();
                }
                else if (commandToLower is "save")
                {
                    WriteLine();
                    WriteLine("Saves the current state of Goat Simulator 3 to a file.");
                    WriteLine();
                    WriteLine("Usage: save path/to/backup.zip");
                    WriteLine("Usage: save");
                    WriteLine();
                    WriteLine("When the path is left blank a save as dialogue will apear to select the save location.");
                    WriteLine();
                }
                else if (commandToLower is "load")
                {
                    WriteLine();
                    WriteLine("Loads a backup file to the current state of Goat Simulator.");
                    WriteLine();
                    WriteLine("Usage: load path/to/backup.zip");
                    WriteLine("Usage: load");
                    WriteLine();
                    WriteLine("When the path is left blank an open file dialogue will apear to select the backup file location.");
                    WriteLine();
                    WriteWarning("The current state of Goat Simulator 3 will be overwritten. Make sure you backup your data first.");
                    WriteLine();
                }
                else if (commandToLower is "launch")
                {
                    WriteLine();
                    WriteLine("Launcehs Goat Simulator 3.");
                    WriteLine();
                    WriteLine("Usage: launch");
                    WriteLine();
                }
                else if (commandToLower is "close")
                {
                    WriteLine();
                    WriteLine("Forceably closes all open instances of Goat Simulator 3.");
                    WriteLine();
                    WriteLine("Usage: close");
                    WriteLine();
                    WriteWarning("Any unsaved progress in Goat Simulator 3 will be lost.");
                    WriteLine();
                }
                else if (commandToLower is "clear")
                {
                    WriteLine();
                    WriteLine("Clears the command history when in shell mode.");
                    WriteLine("Does nothing when ran from the command line arguments.");
                    WriteLine();
                    WriteLine("Usage: clear");
                    WriteLine();
                }
                else if (commandToLower is "exit")
                {
                    WriteLine();
                    WriteLine("Exits the process when in shell mode.");
                    WriteLine("Does nothing when ran from the command line arguments.");
                    WriteLine();
                    WriteLine("Usage: exit");
                    WriteLine();
                }
                else
                {
                    WriteError($"Unknown command \"{command}\". To get help with a command type help followed by its name.");
                }
            }
        }
        //Saves the current state of Goat Simulator 3 to a file.
        public static void SaveCommand(string[] args)
        {
            if (args is null || args.Length is 0)
            {
                Save();
            }
            else if (args.Length is 1)
            {
                Save(args[0], false);
            }
            else
            {
                WriteError($"Too many arguments in command \"Save\". Expected 0 or 1.");
            }
        }
        public static void Save()
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog.CheckPathExists = true;
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.AutoUpgradeEnabled = true;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.Title = "Select destination for Goat Simulator 3 save data backup.";
            saveFileDialog.FileName = $"Goat Simulator 3 Backup {System.DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss").ToLower()}.gsb";
            saveFileDialog.DefaultExt = "gsb";
            saveFileDialog.Filter = "Goat Simulator Backup Files (*.gsb)|*.gsb";
            saveFileDialog.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);

            System.Windows.Forms.DialogResult result = saveFileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Save(saveFileDialog.FileName, true);
            }
            else
            {
                WriteWarning("File save action cancelled by user.");
            }

            saveFileDialog.Dispose();
        }
        public static void Save(string filePath, bool overwriteExisting = false)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo.Extension != ".gsb")
            {
                WriteError($"File must have the .gsb extension. Path \"{filePath}\".");
                return;
            }
            if (fileInfo.Exists && !overwriteExisting)
            {
                WriteError($"File already exists at path \"{filePath}\".");
                return;
            }
            if (!fileInfo.Directory.Exists)
            {
                WriteError($"Selected file resides in a folder which does not exist. Path \"{filePath}\".");
                return;
            }

            string saveDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\Goat2";
            if (!System.IO.Directory.Exists(saveDataFolder))
            {
                WriteError($"Unable to locate Goat Simulator 3 save data folder. Checked in \"{saveDataFolder}\".");
                return;
            }

            if (fileInfo.Exists)
            {
                System.IO.File.Delete(filePath);
            }
            System.IO.Compression.ZipFile.CreateFromDirectory(saveDataFolder, filePath);

            WriteLine();
            WriteLine($"Successfully backed up Goat Simulator 3 save data to \"{filePath}\".");
            WriteLine();
        }
        //Loads a save state of Goat Simulator 3 from a file.
        //Will take a rainy day backup first.
        public static void LoadCommand(string[] args)
        {
            if (args is null || args.Length is 0)
            {
                Load();
            }
            else if (args.Length is 1)
            {
                Load(args[0]);
            }
            else
            {
                WriteError($"Too many arguments in command \"Load\". Expected 0 or 1.");
            }
        }
        public static void Load()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.CheckPathExists = true;
            openFileDialog.FilterIndex = 0;
            openFileDialog.AutoUpgradeEnabled = true;
            openFileDialog.ValidateNames = true;
            openFileDialog.AddExtension = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.Title = "Select source for Goat Simulator 3 save data backup restore.";
            openFileDialog.DefaultExt = "gsb";
            openFileDialog.Filter = "Goat Simulator Backup Files (*.gsb)|*.gsb";
            openFileDialog.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);

            System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Load(openFileDialog.FileName);
            }
            else
            {
                WriteWarning("File load action cancelled by user.");
            }

            openFileDialog.Dispose();
        }
        public static void Load(string filePath)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo.Extension != ".gsb")
            {
                WriteError($"File must have the .gsb extension. Path \"{filePath}\".");
                return;
            }
            if (!fileInfo.Directory.Exists)
            {
                WriteError($"Selected file resides in a folder which does not exist. Path \"{filePath}\".");
                return;
            }
            if (!fileInfo.Exists)
            {
                WriteError($"File does not exist at path \"{filePath}\".");
                return;
            }

            string saveDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\Goat2";
            if (System.IO.Directory.Exists(saveDataFolder))
            {
                string rainyDayPath = $"Rainy Day Goat Simulator 3 Backup {System.DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss").ToLower()}.gsb";

                if (System.IO.File.Exists(rainyDayPath))
                {
                    int rainyDayID = 0;
                    while (System.IO.File.Exists(rainyDayPath))
                    {
                        rainyDayPath = $"Rainy Day Goat Simulator 3 Backup {System.DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss").ToLower()} {rainyDayID}.gsb";
                        rainyDayID++;
                    }
                }

                System.IO.Compression.ZipFile.CreateFromDirectory(saveDataFolder, rainyDayPath);

                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(rainyDayPath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

                System.IO.Directory.Delete(saveDataFolder, true);
            }


            System.IO.Compression.ZipFile.ExtractToDirectory(filePath, saveDataFolder);

            WriteLine();
            WriteLine($"Successfully restored backup of Goat Simulator 3 save data from \"{filePath}\".");
            WriteLine();
        }
        //Launces a new instance of Goat Simulator 3 with the Epic Games Launcher.
        public static void LaunchCommand(string[] args)
        {
            if (args is null || args.Length is 0)
            {
                Launch();
            }
            else
            {
                WriteError($"Too many arguments in command \"Launch\". Expected 0.");
            }
        }
        public static void Launch()
        {
            NoExcept(() =>
            {
                System.Diagnostics.Process.Start("C:\\Program Files\\Epic Games\\GoatSimulator3\\Goat2.exe");
            });
        }
        //Ends the task of all open instances of Goat Simulator 3.
        public static void CloseCommand(string[] args)
        {
            if (args is null || args.Length is 0)
            {
                Close();
            }
            else
            {
                WriteError($"Too many arguments in command \"Close\". Expected 0.");
            }
        }
        public static void Close()
        {
            NoExcept(() => { RunCmdCommand("taskkill", "/f /t /IM \"Goat2-Win64-Shipping.exe\""); });
            NoExcept(() => { RunCmdCommand("taskkill", "/t /IM \"Goat2-Win64-Shipping.exe\""); });
            NoExcept(() => { RunCmdCommand("taskkill", "/f /t /IM \"Goat2-Win32-Shipping.exe\""); });
            NoExcept(() => { RunCmdCommand("taskkill", "/t /IM \"Goat2-Win32-Shipping.exe\""); });
            NoExcept(() => { RunCmdCommand("taskkill", "/f /t /IM \"Goat2.exe\""); });
            NoExcept(() => { RunCmdCommand("taskkill", "/t /IM \"Goat2.exe\""); });
        }
        //Clears the screen when in shell mode.
        public static void ClearCommand(string[] args)
        {
            if (args is null || args.Length is 0)
            {
                Clear();
            }
            else
            {
                WriteError($"Too many arguments in command \"Clear\". Expected 0.");
            }
        }
        public static void Clear()
        {
            System.Console.Clear();
        }
        //Exits from shell mode. Does nothin in command line mode.
        public static void ExitCommand(string[] args)
        {
            if (args is null || args.Length is 0)
            {
                Exit();
            }
            else
            {
                WriteError($"Too many arguments in command \"Exit\". Expected 0.");
            }
        }
        public static void Exit()
        {
            ShellExitRequested = true;
        }
    }
}