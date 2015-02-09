using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SonyAPILib;

namespace Bravia_Controller
{
    public partial class Form_Controler : Form
    {
        const int SWP_NOSIZE = 0x0001;
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();

        SonyAPI_Lib mySonyLib = new SonyAPI_Lib();
        SonyAPI_Lib.SonyDevice mySonyDevice = new SonyAPI_Lib.SonyDevice();
        bool initialized = false, mySonyReg = false; 

        public Form_Controler()
        {
            InitializeComponent();
            AllocConsole();
            IntPtr MyConsole = GetConsoleWindow();
            Console.Title = "Debugger";
            SetWindowPos(MyConsole, 0, this.Location.X + 300, this.Location.Y, 0, 0, SWP_NOSIZE);
            //System.Threading.Thread.Sleep(20000);
        }

        public void Setup(object sender, EventArgs e)
        {
            Console.Clear();
            Console.WriteLine("Buscando dispositivos...");
            List<SonyAPI_Lib.SonyDevice> fDev = mySonyLib.API.sonyDiscover(null);
            Console.WriteLine("Numero de dispositivos: " + fDev.Count);

            if (fDev.Count > 0)
            {
                #region Find Device
                    int i = 0;
                    Console.WriteLine("---------------------------------");
                    foreach (SonyAPI_Lib.SonyDevice fd in fDev)
                    {
                        Console.WriteLine(i + " - " + fd.Name);
                        i = i + 1;
                    }
                    Console.WriteLine("---------------------------------");
                    Console.Write("Introduzca el numero del dispositivo en la lista: ");
                    int n = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("");
                    Console.WriteLine(fDev[n].Name + ": Inicializando....");
                    mySonyDevice.initialize(fDev[n]);
                    Console.WriteLine("Hecho.");
                    initialized = true;
                    System.Threading.Thread.Sleep(1500);
                    Console.Clear();
                #endregion

                #region Register to device 
                    
                    if (mySonyDevice.Registered == false)
                    {
                        Console.Clear();
                        Console.WriteLine("=====================================");
                        Console.WriteLine("Confirme el registro o entre el codigo pin.");
                        Console.WriteLine("=====================================");

                        mySonyReg = mySonyDevice.register();

                        // Check if register returned false
                        if (mySonyDevice.Registered == false)
                        {
                            //Check if Generaton 3. If yes, prompt for pin code
                            if (mySonyDevice.Generation == 3)
                            {
                                string ckii;
                                Console.Write("Inserta el codigo: ");
                                ckii = Console.ReadLine();
                                // Send PIN code to TV to create Autorization cookie
                                Console.WriteLine("Enviando codigo.");
                                mySonyReg = mySonyDevice.sendAuth(ckii);
                            }
                        }
                    } else {  mySonyReg = true; }
                #endregion
                    Commands.Enabled = true;
                    Inf_Disp.Enabled = true;
                    Console.Clear();
            }
            else { Console.WriteLine("There's no Sony Device on the network."); }
        }

        public void Debugger_Control(object sender, EventArgs e)
        {
            ToolStripMenuItem b = (ToolStripMenuItem)sender;
            if (b.Name == "Commands") {
                Console.Clear();
                Console.WriteLine(mySonyDevice.Name + ": Mostrando comandos disponibles");
                string CmdList = mySonyDevice.get_remote_command_list();
                foreach (SonyAPI_Lib.SonyCommands cmd in mySonyDevice.Commands)
                {
                    Console.WriteLine(cmd.name);
                }
            } else if (b.Name == "Inf_Disp") {
                Console.Clear();
                if (mySonyReg)
                {
                    Console.WriteLine("Device Information");
                    Console.WriteLine("Mame: " + mySonyDevice.Name);
                    //Console.WriteLine("Mac Address: " + mySonyDevice.Device_Macaddress);
                    Console.WriteLine("IP Address: " + mySonyDevice.Device_IP_Address);
                    Console.WriteLine("Port: " + mySonyDevice.Device_Port);
                    Console.WriteLine("Generation: " + mySonyDevice.Generation);
                    Console.WriteLine("Registration: " + mySonyDevice.Registered.ToString());
                    Console.WriteLine("Server Name: " + mySonyDevice.Server_Name);
                    //Console.WriteLine("Server Mac: " + mySonyDevice.Server_Macaddress);
                    //Console.WriteLine("Action List URL: " + mySonyDevice.actionList_URL);
                    Console.WriteLine("IRCC Control URL: " + mySonyDevice.control_URL);
                    Console.WriteLine("---------------------------------");
                    Console.WriteLine("");
                }
                else
                {
                    // Display this if NOT true
                    Console.WriteLine("H habido un error");
                    Console.WriteLine("---------------------------------");
                    Console.WriteLine("");
                }
            }
        }

        private void PCControler_Action(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (initialized && mySonyReg)
            {
                Console.Clear();string CmdList = mySonyDevice.get_remote_command_list();
                Console.WriteLine("Boton presionado: {0}",b.Name);
                mySonyDevice.send_ircc(mySonyDevice.getIRCCcommandString(b.Name));
            }
        }
    }
}
