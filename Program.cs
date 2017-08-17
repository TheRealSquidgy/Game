using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Runtime.Serialization;


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
            Shop ItemShop = new Shop();
            if (File.Exists(State.SaveLocation(player)))
            {
                player = (Soldier)State.Load(player);
                playerInventory = (Inventory)State.Load(playerInventory);
            }

            string nextFunction = "Start";
            while (nextFunction != "Exit")
            {
                switch (nextFunction)
                {
                    case "Start":
                        {
                            nextFunction = Start(player);
                            break;
                        }
                    case "CreateSoldier":
                        {
                            nextFunction = CreateSoldier(player, playerInventory);
                            break;
                        }
                    case "Mission":
                        {
                            Entity winner;
                            Sectoid enemy = new Sectoid();
                            Mission.PreCombat(player, enemy);
                            winner = Mission.Combat(player, enemy);
                            Mission.PostCombat(winner, player, playerInventory);
                            nextFunction = "OptionSelect";
                            break;
                        }
                    case "OptionSelect":
                        {
                            nextFunction = OptionSelect(player);
                            break;
                        }
                    case "Shop":
                        {
                            nextFunction = Shop.GoShop(Shop.ShopInventory, player, playerInventory);
                            break;
                        }
                    case "Exit":
                        {
                            break;
                        }
                    default:
                        {
                            Console.Write("This should never run. Something has gone wrong. Press enter to end the session.");
                            Console.ReadLine();
                            System.Environment.Exit(0);
                            break;
                        }
                }
            }
            System.Environment.Exit(0);
        }

        //Game start menu. Player can start a new game or continue an existing one.
        static string Start(Soldier player)
        {            
            string Choice = "";
            while (Choice == "")
            {
                Console.WriteLine("Welcome to Squidgy's Intergalactice Arena of Death! \n");
                Console.WriteLine("Press 1 - Start new game");
                Console.WriteLine("Press 2 - Continue");
                if (File.Exists(State.SaveLocation(player)))
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
                Console.WriteLine("Press 3 - Quit \n");
                Choice = Console.ReadLine();
                switch (Choice)
                {
                    case "1":
                        {
                            Console.Clear();
                            return "CreateSoldier";
                        }
                    case "2":
                        {
                            if (File.Exists(State.SaveLocation(player)))
                            {
                                Console.Clear();
                                return "Mission";
                            }
                            else
                            {
                                Console.WriteLine("No save data! Select 1 to create a new soldier. Press enter to continue");
                                Console.ReadLine();
                                Console.Clear();
                                Choice = "";                               
                                break;
                            }

                        }
                    case "3":
                        {
                            System.Environment.Exit(0);
                            break;
                        }
                        
                    default:
                        {
                            Console.WriteLine("Invalid input. Please enter 1 or 2.");
                            Choice = "";
                            break;
                        }
                }
            }
            return "Mission";
        }

        static string CreateSoldier(Soldier player, Inventory playerInventory)
        {
            //Currently handles naming of new soldiers
            //Will be more useful when we introduce classes and abilities

            if (File.Exists(State.SaveLocation(player)))
            {
                Console.WriteLine("Starting a new game will end the current soldier's career!");
                Console.WriteLine("Are you sure you wish to do this? Y/N");
                string overwrite_save = Console.ReadLine();
                if (overwrite_save.ToUpper() == "Y")
                {
                    File.Delete(State.SaveLocation(player));
                    File.Delete(State.SaveLocation(playerInventory));
                    player = null;
                    player = new Soldier();
                    Console.WriteLine("Save deleted. Creating new soldier.");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Creation cancelled. Returning to start menu.");
                    return "Start";
                }
            }

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
                    State.Save(player);
                    State.Save(playerInventory);
                    Console.Write("{0} saved to file. Press enter to continue.", player.Name);
                    Console.ReadLine();
                    Console.Clear();
                    return "Mission";
                }
                else
                {
                    name_confirmed = "";
                    Console.WriteLine("Name cancelled. Please re-enter your soldier's name.");
                }
            }

            return "Mission";
        }

        //Win or lose, OptionSelect handles what happens after PostCombat. If the player won (savefile still exists), they can play again, visit the item shop or save and quit. If the player lost, they can return to title screen or quit.
        public static string OptionSelect(Soldier player)
        {
            if (File.Exists(State.SaveLocation(player)))
            {
                string Choice = "";
                while (Choice == "")
                {
                    Console.WriteLine("What would you like to do next? \n");
                    Console.WriteLine("1. Play again.");
                    Console.WriteLine("2. Visit the Item Shop.");
                    Console.WriteLine("3. Save and exit.");
                    Choice = Console.ReadLine();
                    switch (Choice)
                    {
                        case "1":
                            {
                                Console.Clear();
                                return "Mission";                               
                                //break;
                            }
                        case "2":
                            {
                                Console.Clear();
                                return "Shop";
                                //break;
                            }
                        case "3":
                            {
                                Console.Clear();
                                return "Exit";
                                //break;
                            }
                        default:
                            {
                                Console.WriteLine("Invalid input. Please enter 1, 2 or 3 from the above options.");
                                Choice = "";
                                break;
                            }

                    }
                }
            }
            else
            {
                string Choice = "";
                while (Choice == "")
                {
                    Console.WriteLine("What would you like to do next?\n");
                    Console.WriteLine("1. Start a new game");
                    Console.WriteLine("2. Quit");
                    Choice = Console.ReadLine();
                    switch (Choice)
                    {
                        case "1":
                            {
                                return "Start";
                                //break;
                            }
                        case "2":
                            {
                                return "Exit";
                                //break;
                            }
                        default:
                            {
                                Console.WriteLine("Invalid input. Please enter 1 or 2.");
                                Choice = "";
                                break;
                            }
                    }
                }
            }
            Console.WriteLine("Something has gon wrong in OptionSelect. You should never see this.");
            return "Exit";
        }
    }

    static class Mission
    {
        //Displays player and enemy stats before the match. May be useful in the future for handling loadouts, selecting team mates, etc
        public static void PreCombat(Soldier player, Sectoid enemy)
        {

            Console.WriteLine("You are {0}, a {1} with {2}HP, armed with a {3} that does {4}-{5} damage.", player.Name, player.Class, player.HPMax, player.WeaponEquipped.Name, player.WeaponEquipped.DmgMin, player.WeaponEquipped.DmgMax);
            Console.WriteLine("Your opponent is a {0} with {1}HP, armed with a {2} that does {3}-{4} damage. \n", enemy.Class, enemy.HPMax, enemy.WeaponEquipped.Name, enemy.WeaponEquipped.DmgMin, enemy.WeaponEquipped.DmgMax);
            Console.WriteLine("Let battle commence! Press enter to continue.\n");
            Console.ReadLine();
            Console.Clear();
        }

        //Combat continues until there is only one survivor, detected and returned by CheckForSurvivors function. The survivor will be announced in PostCombat.
        public static Entity Combat(Soldier player, Sectoid enemy)
        {
            Entity survivor = null;
            while (survivor == null)
            {
                Console.WriteLine("{0} the {1} - {2}/{3}HP", player.Name, player.Class, player.HPCurrent, player.HPMax);
                Console.WriteLine("Enemy {0} - {1}/{2}HP\n", enemy.Name, enemy.HPCurrent, enemy.HPMax);
                Console.WriteLine("Press 1 to Attack");
                string Choice = "";
                while (Choice == "")
                {
                    Choice = Console.ReadLine();
                    switch (Choice)
                    {
                        case "1":
                            bool criticalHit;
                            criticalHit = Attack(player, enemy);
                            survivor = CheckForSurvivors(enemy, player, criticalHit);
                            if (!(survivor == null))
                            {
                                break;
                            }
                            criticalHit = Attack(enemy, player);
                            survivor = CheckForSurvivors(player, enemy, criticalHit);
                            Console.Write("Press enter to continue.");
                            Console.ReadLine();
                            Console.Clear();
                            break;
                        default:
                            //This screenclear might need adjustment.
                            Console.WriteLine("Invalid input. Press 1 to Attack.");
                            //Console.ReadLine();
                            //Console.Clear();
                            Choice = "";
                            break;

                    }
                }

            }
            return survivor;
        }

        public static bool Attack(Entity Attacker, Entity Defender)
        {
            bool attackLands = RollToHit(Attacker);
            bool attackCrits = RollToCrit(Attacker);
            if (attackLands)
            {
                int damage = CalculateDamage(Attacker, attackCrits);
                Defender.HPCurrent -= damage;
                if (attackCrits)
                {
                    Console.WriteLine("{0} lands a devastating critical hit on the {1}, doing {2} damage!", Attacker.Name, Defender.Class, damage);
                    return true;
                }
                else
                {
                    Console.WriteLine("{0} fires their {1} at the {2}, doing {3} damage!", Attacker.Name, Attacker.WeaponEquipped.Name, Defender.Name, damage);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("{0} fires at the {1}...and misses!", Attacker.Name, Defender.Class);
                return false;
            }
        }

        public static bool RollToHit(Entity Attacker)
        {
            Random random = new Random();
            int rollToHit = random.Next(1, 101);
            if (rollToHit <= Attacker.Aim)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool RollToCrit(Entity Attacker)
        {
            Random random = new Random();
            int rollToCrit = random.Next(1, 101);
            if (rollToCrit <= Attacker.WeaponEquipped.CritChance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int CalculateDamage(Entity Attacker, bool AttackCrits)
        {
            Thread.Sleep(100);
            if (AttackCrits)
            {
                Random random1 = new Random();
                int CritDamage = random1.Next(Attacker.WeaponEquipped.CritDmgMin, (Attacker.WeaponEquipped.CritDmgMax + 1));
                return CritDamage;
            }
            else
            {
                Random random2 = new Random();
                int Damage = random2.Next(Attacker.WeaponEquipped.DmgMin, (Attacker.WeaponEquipped.DmgMax + 1));
                return Damage;
            }
        }

        //Checks if damage to victim is fatal, and if so whether it was a critical attack. If so, return the victor.
        public static Entity CheckForSurvivors(Entity Victim, Entity Victor, bool CriticalHit)
        {
            if (Victim.HPCurrent <= 0)
            {
                if (CriticalHit)
                {
                    Console.WriteLine("BOOM headshot! The {0} splatters the {1}'s brains all over the arena, killing them instantly. The match is over!", Victor.Class, Victim.Class);
                }
                else
                {
                    Console.WriteLine("The {0} cuts down the {1} with a withering hail of gunfire. The match is over!", Victor.Class, Victim.Class);
                }
                Console.WriteLine("Press enter to continue.");
                Console.ReadLine();
                Console.Clear();
                return Victor;
            }
            else
            {
                return null;
            }
        }

        //Check if a soldier has won. If so, hand out post-combat rewards to the player. If not, delete the player character. Once the winner has been handled, return OptionSelect.
        public static string PostCombat(Entity Winner, Soldier player, Inventory playerInventory)
        {
            if (Winner.Class == "Soldier")
            {
                Console.WriteLine("Congratulations, you win!");
                int experienceEarned = GenerateExperience();
                Console.WriteLine("{0} earned {1} experience points!", player.Name, experienceEarned);
                player.Experience += experienceEarned;
                while (player.Experience >= player.ExpReqToLevel)
                {
                    int ExperienceCarriedOver = player.Experience - player.ExpReqToLevel;
                    player.Experience = ExperienceCarriedOver;
                    player.Level++;
                    Console.WriteLine("You are now level {0}!", player.Level);
                    player.ExpReqToLevel = CalculateNewLevelExpReq(player);
                }
                int prize_money = GeneratePrizeMoney();
                player.Moonbux += prize_money;
                Console.WriteLine("{0} wins {1} Moonbux as prize money! \n", player.Name, prize_money);
                int nextLevel = player.Level + 1;
                Console.WriteLine("Progress to level {0}: {1}/{2} \t Moonbux: {3}", nextLevel, player.Experience, player.ExpReqToLevel, player.Moonbux);
                player.HPCurrent = player.HPMax;
                player.Survived += 1;
                State.Save(player);
                Console.WriteLine("{0}'s progress has been saved.", player.Name);
            }
            else
            {
                Console.WriteLine(player.Name + " is dead! Game over.");
                Console.WriteLine("!!!---SAVE DELETED---!!!");
                File.Delete(State.SaveLocation(player));
                File.Delete(State.SaveLocation(playerInventory));
            }
            return "OptionSelect";
        }

        public static int GenerateExperience()
        {
            Random experienceGain = new Random();
            int Experience = experienceGain.Next(80, 121);
            return Experience;
        }

        public static int GeneratePrizeMoney()
        {
            Random randomPayout = new Random();
            int prizeMoney = randomPayout.Next(100, 201);
            return prizeMoney;
        }

        static int CalculateNewLevelExpReq(Soldier player)
        {
            int LevelExpReq = 100 + (player.Level * 50);
            return LevelExpReq;
        }

    }

    class Shop
    {
        public static List<ShopItem> ShopInventory = new List<ShopItem>();

        public Shop()
        {
            ShopInventory.Add(new ShopItem { WeaponData = new AssaultRifle(), ItemPrice = 0 });
            ShopInventory.Add(new ShopItem { WeaponData = new Shotgun(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new SniperRifle(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new LMG(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new RocketLauncher(), ItemPrice = 1000 });
            ShopInventory.Add(new ShopItem { WeaponData = new Minigun(), ItemPrice = 1000 });
        }

        public static string GoShop(List<ShopItem> ShopInventory, Soldier player, Inventory playerInventory)
        {
            string itemChoice = "";
            while (itemChoice == "")
            {
                Console.Clear();
                Console.WriteLine("Welcome to the item shop!\n"); //displays item list, prompts user to select item number
                for (int i = 0; i < ShopInventory.Count; i++)
                {
                    Console.WriteLine((i + 1) + ") {0} - {1} Moonbux", ShopInventory[i].WeaponData.Name, ShopInventory[i].ItemPrice);
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
                                                                Console.ReadLine();
                                                                playerInventory.Weapons.Add(player.WeaponEquipped);
                                                                player.WeaponEquipped = selectedWeaponData;
                                                                itemChoice = "";
                                                                break;
                                                            }
                                                        case "2":
                                                            {
                                                                playerInventory.Weapons.Add(selectedWeaponData);
                                                                Console.WriteLine(selectedWeaponData.Name + " sent to inventory. Press enter to return to item shop.");
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
                                Console.WriteLine(selectedWeaponData.Name + " already equipped. Press enter to return to the item list.");
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
                                                Console.ReadLine();
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
                                                Console.ReadLine();
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
                    State.Save(playerInventory);
                    Console.WriteLine("Inventory has been saved.");
                    Console.ReadLine();
                    Console.Clear();
                    return "OptionSelect";
                }
                else
                {
                    //Console.Clear();
                    Console.WriteLine("Invalid input. Select a valid number to view weapon stats.");
                    itemChoice = "";
                    Console.ReadLine();
                }
            }
            return "OptionSelect";
        }

        static void RemoveWeaponFromInventory(Soldier player, Inventory playerInventory)
        {
            //Searches the player inventory for the same weapon that the player has equipped and removes it from inventory.
            for (int i = 0; i < playerInventory.Weapons.Count; i++)
            {
                if (playerInventory.Weapons[i].Name == player.WeaponEquipped.Name)
                {
                    playerInventory.Weapons.RemoveAt(i);
                }
            }
        }

        static bool CheckWeaponOwnership(Inventory playerInventory, Soldier player, Weapon selectedItem)
        {
            if (player.WeaponEquipped.Name == selectedItem.Name)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < playerInventory.Weapons.Count; i++)
                {
                    if (playerInventory.Weapons[i].Name == selectedItem.Name)
                    {
                        return true;
                    }
                }
            }
            return false;
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

    //This class deals data handling; Saving and loading game states.
    static class State
    {
        public static string SaveLocation(Object SaveData)
        {
            if (SaveData is Soldier)
            {
                string SoldierSaveLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "soldier.xml");
                return SoldierSaveLocation;
            }
            else if (SaveData is Inventory)
            {
                string InventorySaveLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventory.xml");
                return InventorySaveLocation;
            }
            else
            {
                Console.WriteLine("Something has gone wrong in the Save function. This should never appear. Writing inventory data as fallback.");
                string InventorySaveLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventory.xml");
                return InventorySaveLocation;
            }

        }

        public static void Save(Object SaveData)
        {
            using (Stream stream = new FileStream(SaveLocation(SaveData), FileMode.Create))
            {
                DataContractSerializer serializer = new DataContractSerializer(SaveData.GetType());
                serializer.WriteObject(stream, SaveData);
            }
        }

        //When using object you need to cast the type on the loaded object e.g. player = (Soldier)State.Load(player);
        public static Object Load(Object SaveData)
        {

            DataContractSerializer Deserializer = new DataContractSerializer(SaveData.GetType());
            using (Stream stream = File.OpenRead(SaveLocation(SaveData)))
            {
                Object retrievedState = Deserializer.ReadObject(stream);
                return retrievedState;
            }
        }
    }

    [DataContract]
    [KnownType(typeof(AssaultRifle))]
    [KnownType(typeof(PlasmaPistol))]
    [KnownType(typeof(Shotgun))]
    [KnownType(typeof(SniperRifle))]
    [KnownType(typeof(LMG))]
    [KnownType(typeof(RocketLauncher))]
    [KnownType(typeof(Minigun))]
    public class Entity
    {
        [DataMember] public string Name;
        [DataMember] public string Class;
        [DataMember] public int HPCurrent;
        [DataMember] public int HPMax;
        [DataMember] public int Aim;
        [DataMember] public Weapon WeaponEquipped;
    }

    [DataContract]
    public class Soldier : Entity
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

        [DataMember] public int Level = 1;
        [DataMember] public int Experience = 0;
        [DataMember] public int ExpReqToLevel = 100;
        [DataMember] public int Moonbux = 0;
        [DataMember] public int Survived = 0;
    }

    public class Sectoid : Entity
    {
        public Sectoid()
        {
            Name = "Sectoid";
            Class = "Sectoid";
            HPCurrent = 70;
            HPMax = 70;
            Aim = 70;
            WeaponEquipped = new PlasmaPistol();
        }
    }

    //player inventory. Currently only stores weapons. This needs to be expanded as items come online.
    [DataContract]
    [KnownType(typeof(AssaultRifle))]
    [KnownType(typeof(PlasmaPistol))]
    [KnownType(typeof(Shotgun))]
    [KnownType(typeof(SniperRifle))]
    [KnownType(typeof(LMG))]
    [KnownType(typeof(RocketLauncher))]
    [KnownType(typeof(Minigun))]
    public class Inventory
    {
        public Inventory()
        {
            Weapons = new List<Weapon>();
        }
        [DataMember] public List<Weapon> Weapons;
    }

    [DataContract]
    public class Weapon
    {
        [DataMember] public string Name;
        [DataMember] public int CritChance;
        [DataMember] public int DmgMin;
        [DataMember] public int DmgMax;
        [DataMember] public int CritDmgMin;
        [DataMember] public int CritDmgMax;
        [DataMember] public string Description;
    }

    [DataContract]
    public class AssaultRifle : Weapon
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

    [DataContract]
    public class PlasmaPistol : Weapon
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

    [DataContract]
    public class Shotgun : Weapon
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

    [DataContract]
    public class SniperRifle : Weapon
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

    [DataContract]
    public class LMG : Weapon
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

    [DataContract]
    public class RocketLauncher : Weapon
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

    [DataContract]
    public class Minigun : Weapon
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

    public class ShopItem
    {
        public Weapon WeaponData;
        public int ItemPrice;
    }
}

