using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

/*
v1.5 - Code refactor! Change save files to XML filetype, create orchestrator-style switch to allow dynamic function ordering, 
v1.4 - Introduce new RPG mechanics. A levelling system, critical hits. End-of-round shop function created but not implemented.
v1.3 - Introduced save files - binary filetype.
v1.2 - Made combat repeatable
*/

namespace Gamev1._5
{
    class Program
    {
        static void Main(string[] args)
        {
            Soldier player = new Soldier();
            Inventory playerInventory = new Inventory();
            if (File.Exists(State.SaveLocation))
            {
                player = State.Load(player);
            }

            string NextFunction = "Start";
            while (NextFunction != "Exit")
            {
                switch(NextFunction)
                {
                    case "Start":
                        {
                            NextFunction = Start(player);
                            break;
                        }
                    case "PreCombat":
                        {
                            PreCombat(player);
                            break;
                        }
                    //case "Combat":
                    //    {
                    //        Combat();
                    //        break;
                    //    }
                    //case "PostCombat":
                    //    {
                    //        PostCombat();
                    //        break;
                    //    }
                    //case "GoShopping":
                    //    {
                    //        GoShopping();
                    //        break;
                    //    }
                    default:
                        {
                            Console.Write("This should never run. Something has gone wrong.");
                            Console.ReadLine();
                            Exit(player);
                            break;
                        }
                }
            }
            Exit(player);
        }

        static string Start(Soldier player)
        {
            Console.WriteLine("Welcome to Squidgy's Intergalactice Arena of Death! \n");
            Console.WriteLine("Press 1 to create a new soldier");
            Console.WriteLine("Press 2 to continue a soldier's career");
            if (File.Exists(State.SaveLocation))
            {
                Console.WriteLine("\t Name: " + player.Name);
                Console.WriteLine("\t Level: " + player.Level);
                Console.WriteLine("\t Career winnings: " + player.Moonbux + " Moonbux");
                Console.WriteLine("\t Missions survived: " + player.Survived);
            }
            else
            {
                Console.WriteLine("\t---NO SAVE DATA---");
            }
            string Choice = "";
            while (Choice == "")
            {
                Choice = Console.ReadLine();
                switch(Choice)
                {
                    case "1":
                        {
                            if (File.Exists(State.SaveLocation))
                            {
                                Console.WriteLine("Creating a new soldier will end the current soldier's career!");
                                Console.WriteLine("Are you sure you wish to do this? Y/N");
                                string overwrite_save = Console.ReadLine();
                                if (overwrite_save.ToUpper() == "Y")
                                {
                                    File.Delete(State.SaveLocation);
                                    CreateSoldier(player);
                                    State.Save(player);
                                    Console.WriteLine(player.Name + " saved to file. \n");
                                }
                                else
                                {
                                    Console.WriteLine("Creation cancelled.");
                                    Console.WriteLine("Press 1 to create a new soldier");
                                    Console.WriteLine("Press 2 to continue a soldier's career");
                                    Choice = "";
                                }
                            }
                            else
                            {
                                CreateSoldier(player);
                            }
                            break;
                        }
                    case "2":
                        {
                            if (File.Exists(State.SaveLocation))
                            {
                                return "PreCombat";
                                //break;
                            }
                            else
                            {
                                Console.WriteLine("No save data! Please select 1 to create a new soldier.");
                                Choice = "";
                                break;
                            }

                        }
                    default:
                        {
                            Console.WriteLine("Invalid input. Please enter 1 or 2.");
                            Choice = "";
                            break;
                        }
                }
            }
            return "PreCombat";
        }

        static string CreateSoldier(Soldier player)
        {
            //Currently handles naming of new soldiers
            //Will be more useful when we introduce classes and abilities
            string name_confirmed = "";
            while (name_confirmed == "")
            {
                Console.WriteLine("Please enter the name of your soldier: ");
                string soldier_name = Console.ReadLine();
                Console.WriteLine("Do you wish you call your soldier " + soldier_name + "? Y/N");
                name_confirmed = Console.ReadLine();
                if (name_confirmed == "y" || name_confirmed == "Y")
                {
                    player.Name = soldier_name;
                    Console.WriteLine("Your soldier is now called " + player.Name + "!");
                    return "PreCombat";
                }
                else
                {
                    name_confirmed = "";
                    Console.WriteLine("Name cancelled. Please re-enter your soldier's name.");
                }
            }
            return "PreCombat";
        }

        static void PreCombat(Soldier player)
        {
            Console.WriteLine("This is the precombat phase! Your soldier's name is " + player.Name);
            Console.ReadLine();
            System.Environment.Exit(1);
        }

        static void Exit(Soldier player)
        {
            //Saves and exits the game. The save process at this stage is cautionary and optional as the last function to write to the save file should have saved on exit anyway.
            State.Save(player);
            System.Environment.Exit(0);
        }
    }

    static class State
    {
        //The following variable requires System.IO to make Path.Combine work
        public static string SaveLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.xml");

        public static void Save(Soldier player)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Soldier));
            using (TextWriter writer = new StreamWriter(SaveLocation))
            {
                serializer.Serialize(writer, player);
            }
        }

        public static Soldier Load(Soldier player)
        {
            //Instantiates Xmlserialiser with the object type to deserialize
            XmlSerializer deserializer = new XmlSerializer(typeof(Soldier));
            //Reads the file using FileStream
            FileStream myFileStream = new FileStream(SaveLocation, FileMode.Open);
            //Calls the Deserialize method and casts it to object type
            Soldier RetrievedSaveState = (Soldier)deserializer.Deserialize(myFileStream);
            return RetrievedSaveState;
        }
    }

    [Serializable]
    class Entity
    {
        public string Name;
        public string Class;
        public int HPCurrent;
        public int HPMax;
        public int Aim;
        public Weapon WeaponEquipped;
    }

    [Serializable]
    class Soldier : Entity
    {
        public Soldier()
        {
            Name = "";
            Class = "Soldier";
            HPCurrent = 80;
            HPMax = 80;
            Aim = 75;
            WeaponEquipped = new AssaultRifle();
        }

        public int Level = 1;
        public int Experience = 0;
        public int ExpReqToLevel = 100;
        public int Moonbux = 0;
        public int Survived = 0;

    }

    [Serializable]
    class Inventory
    {
        public Inventory()
        {
            Weapons = new List<Weapon>();
        }

        public List<Weapon> Weapons;
    }

    class Sectoid : Entity
    {
        public Sectoid()
        {
            Name = "Sectoid";
            Class = "Sectoid";
            HPCurrent = 70;
            HPMax = 70;
            Aim = 60;
            WeaponEquipped = new PlasmaPistol();
        }
    }

    [Serializable]
    class Weapon
    {
        public string Name;
        public int CritChance;
        public int DmgMin;
        public int DmgMax;
        public int CritDmgMin;
        public int CritDmgMax;
        public string Description;
    }

    [Serializable]
    class AssaultRifle : Weapon
    {
        public AssaultRifle()
        {
            Name = "Assault Rifle";
            CritChance = 10;
            DmgMin = 20;
            DmgMax = 40;
            CritDmgMin = 50;
            CritDmgMax = 70;
            Description = "Standard issue firearm for all soldiers.";
        }
    }

    [Serializable]
    class PlasmaPistol : Weapon
    {
        public PlasmaPistol()
        {
            Name = "Plasma Pistol";
            CritChance = 0;
            DmgMin = 20;
            DmgMax = 40;
            CritDmgMin = 50;
            CritDmgMax = 70;
            Description = "An alien weapon. Small but powerful for a pistol.";
        }
    }

    [Serializable]
    class Shotgun : Weapon
    {
        public Shotgun()
        {
            Name = "Shotgun";
            CritChance = 20;
            DmgMin = 30;
            DmgMax = 50;
            CritDmgMin = 60;
            CritDmgMax = 80;
            Description = "Close range assault weapon with high crit damage.";
        }
    }

    [Serializable]
    class SniperRifle : Weapon
    {
        public SniperRifle()
        {
            Name = "Sniper Rifle";
            CritChance = 25;
            DmgMin = 30;
            DmgMax = 50;
            CritDmgMin = 60;
            CritDmgMax = 80;
            Description = "Long range bolt-action weapon with good crit chance.";
        }
    }

    [Serializable]
    class LMG : Weapon
    {
        public LMG()
        {
            Name = "LMG";
            CritChance = 10;
            DmgMin = 20;
            DmgMax = 60;
            CritDmgMin = 60;
            CritDmgMax = 80;
            Description = "The high recoil and firing rate gives this unpredictable weapon a wide damage range.";
        }
    }

    [Serializable]
    class RocketLauncher : Weapon
    {
        public RocketLauncher()
        {
            Name = "Rocket Launcher";
            CritChance = 0;
            DmgMin = 60;
            DmgMax = 60;
            CritDmgMin = 0;
            CritDmgMax = 0;
            Description = "Very powerful anti-tank weapon, its explosive payload ensures consistent high damage.";
        }
    }

    [Serializable]
    class Minigun : Weapon
    {
        public Minigun()
        {
            Name = "Minigun";
            CritChance = 10;
            DmgMin = 30;
            DmgMax = 70;
            CritDmgMin = 80;
            CritDmgMax = 100;
            Description = "A man-portable version of an aircraft weapon system. Unmatched firepower and fire rate!";
        }
    }

    class ShopItem
    {
        public Weapon WeaponData;
        public int ItemPrice;
    }
}
