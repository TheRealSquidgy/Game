//This is the shop function. It needs to be integrated into the main game code!

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ShopItem ItemShop = new ShopItem();
            Soldier player = new Soldier();
            Inventory playerInventory = new Inventory();
            player.Name = "Squidgy";
            player.Moonbux = 1000;
            GoShop(ItemShop, player, playerInventory);
            Console.ReadLine();
        }

        static void GoShop(ShopItem ItemShop, Soldier player, Inventory playerInventory)
        {
            List<ShopItem> ShopInventory = new List<ShopItem>(); //Populates ShopInventory at runtime. Is there another way to handle this?           
            ShopInventory.Add(new ShopItem { WeaponData = new AssaultRifle(), ItemPrice = 0 });
            ShopInventory.Add(new ShopItem { WeaponData = new Shotgun(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new SniperRifle(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new LMG(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new RocketLauncher(), ItemPrice = 1000 });
            ShopInventory.Add(new ShopItem { WeaponData = new Minigun(), ItemPrice = 1000 });

            string itemChoice = "";
            while (itemChoice == "")
            {
                Console.Clear();
                Console.WriteLine("Welcome to the item shop!\n"); //displays item list, prompts user to select item number
                for (int i = 0; i < ShopInventory.Count; i++)
                {
                    Console.WriteLine((i + 1) + ") " + ShopInventory[i].WeaponData.Name);
                }
                Console.WriteLine();
                Console.WriteLine(player.Name + "'s funds: " + player.Moonbux + " Moonbux");
                Console.WriteLine("Select a number to view weapon stats. Type EXIT to leave the item shop: -");

                itemChoice = Console.ReadLine();
                bool isInt;
                int intItemChoice;
                isInt = int.TryParse(itemChoice, out intItemChoice);           

                if (isInt)
                {
                    intItemChoice -= 1;
                    if (intItemChoice <= ShopInventory.Count && intItemChoice >= 0) //checks if number is valid menu option. If yes, display item stats.
                    {
                        Console.Clear();

                        Weapon selectedWeaponData = ShopInventory[intItemChoice].WeaponData;

                        Console.WriteLine("Weapon: " + selectedWeaponData.Name);
                        Console.WriteLine("Damage: " + selectedWeaponData.DmgMin + "-" + selectedWeaponData.DmgMax);
                        Console.WriteLine("Critical Hit Damage: " + selectedWeaponData.CritDmgMin + "-" + selectedWeaponData.CritDmgMax);
                        Console.WriteLine("Critical Hit Chance: " + selectedWeaponData.CritChance + "%");
                        Console.WriteLine(selectedWeaponData.Description + "\n");

                        bool playerOwnsWeapon = CheckWeaponOwnership(playerInventory, player, selectedWeaponData);
                        if (playerOwnsWeapon == false) //if player does not own the selected weapon give them the choice to buy it, assuming they have the funds.
                        {
                            Console.WriteLine("Press 1 to buy weapon. Press 2 to return to item list.");
                            string purchaseDecision = "";                            
                            while (purchaseDecision == "")
                            {
                                purchaseDecision = Console.ReadLine();
                                switch (purchaseDecision)
                                {
                                    case "1":
                                        {
                                            bool playerCanAffordItem = CheckFunds(player, ShopInventory[intItemChoice]);
                                            if (playerCanAffordItem)
                                            {
                                                player.Moonbux -= ShopInventory[intItemChoice].ItemPrice;
                                                Console.WriteLine("Item purchased! Press 1 to equip now. Press 2 to send to inventory.");
                                                string purchasedItemDestination = "";
                                                while (purchasedItemDestination == "")
                                                {
                                                    purchasedItemDestination = Console.ReadLine();
                                                    switch (purchasedItemDestination)
                                                    {
                                                        case "1":
                                                            {
                                                                Console.WriteLine(selectedWeaponData.Name + " equipped. " + player.WeaponEquipped.Name + " sent to inventory.");
                                                                playerInventory.Weapons.Add(player.WeaponEquipped);
                                                                player.WeaponEquipped = selectedWeaponData;
                                                                itemChoice = "";
                                                                break;
                                                            }
                                                        case "2":
                                                            {
                                                                playerInventory.Weapons.Add(selectedWeaponData);
                                                                Console.WriteLine(selectedWeaponData.Name + " sent to inventory. Press any key to return to item shop.");
                                                                Console.ReadLine();
                                                                itemChoice = "";
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                Console.WriteLine("Invalid input. Press 1 to equip now. Press 2 to send to inventory.");
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Insufficient funds! Returning to item shop.");
                                                Console.ReadLine();
                                                itemChoice = "";
                                            }
                                            break;
                                        }
                                    case "2":
                                        {
                                            itemChoice = "";
                                            break;
                                        }
                                    default:
                                        {
                                            Console.WriteLine("Invalid input. Press 1 to buy weapon. Press 2 to return to item list.");
                                            purchaseDecision = "";
                                            break;  
                                        }
                                }
                            }
                        }
                        else //if player does own the weapon, check if equipped. If not, offer to equip the weapon.
                        {
                            bool selectedWeaponAlreadyEquipped;
                            selectedWeaponAlreadyEquipped = CheckIfEquipped(player, selectedWeaponData);
                            if (selectedWeaponAlreadyEquipped)
                            {                              
                                Console.WriteLine(selectedWeaponData.Name + " already equipped. Press any key to return to the item list.");
                                Console.ReadLine();
                                itemChoice = "";
                            }
                            else
                            {
                                Console.WriteLine(selectedWeaponData.Name + " already purchased and in storage.");
                                Console.WriteLine("You currently have the " + player.WeaponEquipped.Name + " equipped.");
                                Console.WriteLine("Press 1 to swap out weapons. Press 2 to return to item list.");
                                string equipOrReturn = "";
                                while (equipOrReturn == "")
                                {
                                    equipOrReturn = Console.ReadLine();
                                    switch (equipOrReturn)
                                    {
                                        case "1":
                                            {
                                                playerInventory.Weapons.Add(player.WeaponEquipped);
                                                player.WeaponEquipped = selectedWeaponData;
                                                RemoveWeaponFromInventory(player, playerInventory);
                                                Console.WriteLine(player.WeaponEquipped.Name + " taken from inventory and equipped.");
                                                itemChoice = "";
                                                break;
                                            }
                                        case "2":
                                            {
                                                itemChoice = "";
                                                break;
                                            }
                                        default:
                                            {
                                                Console.WriteLine("Invalid input. Press 1 to equip this weapon now. Press 2 to return to item list.");
                                                equipOrReturn = "";
                                                break;
                                            }
                                    }
                                }

                            }
                        }

                    }
                    else
                    {
                        //Console.Clear();
                        Console.WriteLine("Invalid input. Select a valid number to view weapon stats.");
                        itemChoice = "";
                        Console.ReadLine();
                    }
                }
                else if (itemChoice.ToLower() == "exit")
                {
                    Console.WriteLine("Thanks for visiting the item shop! See you next time.");
                    Console.WriteLine("!!!---PLAYER EXITS THE STORE---!!!");
                    Console.ReadLine();
                    System.Environment.Exit(1);
                }
                else
                {
                    //Console.Clear();
                    Console.WriteLine("Invalid input. Select a valid number to view weapon stats.");
                    itemChoice = "";
                    Console.ReadLine();
                }
            }
        }

        static void RemoveWeaponFromInventory(Soldier player, Inventory playerInventory)
        {
            //Searches the player inventory for the same weapon that the player has equipped and removes it from inventory.
            for (int i = 0; i < playerInventory.Weapons.Count; i++)
            {
                if (playerInventory.Weapons[i] == player.WeaponEquipped)
                {
                    playerInventory.Weapons.RemoveAt(i);
                }
            }
        }

        static bool CheckWeaponOwnership(Inventory PlayerInventory, Soldier player, Weapon selectedItem)
        {
            if (player.WeaponEquipped.Name == selectedItem.Name)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < PlayerInventory.Weapons.Count; i++)
                {
                    if (PlayerInventory.Weapons[i].Name == selectedItem.Name)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false; //this should never trigger.
        }

        static bool CheckFunds(Soldier player, ShopItem selectedItem)
        {
            if (player.Moonbux >= selectedItem.ItemPrice)
                return true;
            else
                return false;
        }

        static bool CheckIfEquipped(Soldier player, Weapon selectedItem)
        {
            if (player.WeaponEquipped.Name == selectedItem.Name)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            Description = "Standard issue for all soldiers.";
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
            Description = "An alien weapon. Small but powerful.";
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
            CritDmgMin = 70;
            CritDmgMax = 90;
            Description = "Close range weapon with high crit damage.";
        }
    }

    [Serializable]
    class SniperRifle : Weapon
    {
        public SniperRifle()
        {
            Name = "Sniper Rifle";
            CritChance = 30;
            DmgMin = 30;
            DmgMax = 50;
            CritDmgMin = 60;
            CritDmgMax = 80;
            Description = "Long range precision weapon with good crit chance.";
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
            Description = "The high fire rate and recoil gives this unpredictable weapon a wide damage range.";
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
            CritChance = 20;
            DmgMin = 40;
            DmgMax = 70;
            CritDmgMin = 80;
            CritDmgMax = 100;
            Description = "A man-portable version of an aircraft weapon system. Unmatched firepower!";
        }
    }

    class ShopItem
    {
        //public string ItemID;
        public Weapon WeaponData;
        public int ItemPrice;
    }
}
