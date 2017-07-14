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
            if (File.Exists(State.SaveLocation))
            {
                player = State.Load(player);
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
                    case "Mission":
                        {
                            Entity winner;
                            Sectoid enemy = new Sectoid();
                            Mission.PreCombat(player, enemy);
                            winner = Mission.Combat(player, enemy);
                            Mission.PostCombat(winner, player);
                            nextFunction = "OptionSelect";
                            break;
                        }
                    case "OptionSelect":
                        {
                            nextFunction = OptionSelect();
                            break;
                        }
                    case "Shop":
                        {
                            bool playerExitsShop = false;
                            while (playerExitsShop == false)
                            {
                                int itemSelected;
                                itemSelected = ItemShop.BrowseItemShop(player);
                                switch(itemSelected)
                                {
                                    case 9999:
                                        {
                                            playerExitsShop = true;
                                            break;
                                        }
                                    default:
                                        {
                                            ItemShop.ExamineItem(itemSelected, player, playerInventory);                                           
                                            break;
                                        }
                                }
                            }
                            Console.WriteLine("Thanks for visiting the store. See you later!");
                            nextFunction = "OptionSelect";                          
                            break;
                        }
                    default:
                        {
                            Console.Write("This should never run. Something has gone wrong. Press enter to end the session.");
                            Console.ReadLine();
                            //Exit(player);
                            System.Environment.Exit(0);
                            break;
                        }
                }
            }
            //Exit(player);
            System.Environment.Exit(0);
        }

        //Game start menu. Player can start a new game or continue an existing one.
        static string Start(Soldier player)
        {
            Console.WriteLine("Welcome to Squidgy's Intergalactice Arena of Death! \n");
            Console.WriteLine("Press 1 - Start new game");
            Console.WriteLine("Press 2 - Continue");
            Console.WriteLine("Press 3 - Quit");
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
                switch (Choice)
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
                                    Console.WriteLine("Press 1 to start a new game");
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
                                return "Mission";
                                //break;
                            }
                            else
                            {
                                Console.WriteLine("No save data! Please select 1 to create a new soldier.");
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
                    Console.Write("Press any key to continue.");
                    Console.ReadLine();
                    Console.Clear();
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

        //Win or lose, OptionSelect handles what happens after PostCombat. If the player won (savefile still exists), they can play again, visit the item shop or save and quit. If the player lost, they can return to title screen or quit.
        public static string OptionSelect()
        {
            if (File.Exists(State.SaveLocation))
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
                                return "Mission";
                                //break;
                            }
                        case "2":
                            {
                                return "Shop";
                                //break;
                            }
                        case "3":
                            {
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
            Console.WriteLine("Let battle commence! \n");
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
                return Victor;
            }
            else
            {
                return null;
            }
        }

        //Check if a soldier has won. If so, hand out post-combat rewards to the player. If not, delete the player character. Once the winner has been handled, return OptionSelect.
        public static string PostCombat(Entity Winner, Soldier player)
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
                File.Delete(State.SaveLocation);
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
        List<ShopItem> ShopInventory = new List<ShopItem>();
        /*
        //Displays a list of weapons. The function will return either a valid ShopInventory index value, or it will return 9999, an exit code.
        public int BrowseItemShop(Soldier player)
        {
            string itemChoice = "";
            while (itemChoice == "")
            {
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

                while (itemChoice == "")
                {
                    if (isInt)
                    {
                        intItemChoice -= 1;
                        if (intItemChoice <= ShopInventory.Count && intItemChoice >= 0)
                        {
                            return intItemChoice;
                        }
                    }
                    else if (itemChoice.ToUpper() == "Exit")
                    {
                        Console.WriteLine("Thanks for visiting the item shop! See you next time.");
                        return 9999;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Select a valid menu option to view weapon stats.");
                        itemChoice = "";
                        Console.ReadLine();
                    }
                }

            }
            Console.WriteLine("Something has gone wrong in the DisplayItems function. You should never see this.");
            return 666;
        }

        public void ExamineItem(int indexValue, Soldier player, Inventory playerInventory)
        {
            Weapon selectedWeaponData = ShopInventory[indexValue].WeaponData;

            Console.WriteLine("Weapon: " + selectedWeaponData.Name);
            Console.WriteLine("Damage: " + selectedWeaponData.DmgMin + "-" + selectedWeaponData.DmgMax);
            Console.WriteLine("Critical Hit Damage: " + selectedWeaponData.CritDmgMin + "-" + selectedWeaponData.CritDmgMax);
            Console.WriteLine("Critical Hit Chance: " + selectedWeaponData.CritChance + "%");
            Console.WriteLine(selectedWeaponData.Description + "\n");

            bool playerOwnsWeapon = CheckWeaponOwnership(player, playerInventory, selectedWeaponData);
            if (playerOwnsWeapon == false)
            {
                bool playerCanAffordItem = CheckFunds(ShopInventory[indexValue], player);
                if (playerCanAffordItem)
                {

                }
                else
                {
                    Console.WriteLine("Insufficient funds! Returning to item shop.");
                }

            }
            else
            {

            }
        }

        static bool CheckFunds(ShopItem item, Soldier player)
        {
            if (item.ItemPrice <= player.Moonbux)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //Checks if player has the currently selected weapon equipped, or if they own the weapon in their inventory. If so return true, else return false.
        static bool CheckWeaponOwnership(Soldier player, Inventory playerInventory, Weapon SelectedWeapon)
        {
            if (player.WeaponEquipped.Name == SelectedWeapon.Name)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < playerInventory.Weapons.Count; i++)
                {
                    if (playerInventory.Weapons[i].Name == SelectedWeapon.Name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        */
        public Shop()
        {        
            ShopInventory.Add(new ShopItem { WeaponData = new AssaultRifle(), ItemPrice = 0 });
            ShopInventory.Add(new ShopItem { WeaponData = new Shotgun(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new SniperRifle(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new LMG(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new RocketLauncher(), ItemPrice = 1000 });
            ShopInventory.Add(new ShopItem { WeaponData = new Minigun(), ItemPrice = 1000 });
        }

        //displays ShopInventory, prompts user to enter item number OR exit the store. Will keep looping until player makes a valid selection.
        public int BrowseItemShop(Soldier player)
        {
            bool isInt = false;
            bool validInt = false;
            while (isInt == false && validInt == false)
            {
                Console.WriteLine("Welcome to the item shop!\n");
                for (int i = 0; i < ShopInventory.Count; i++)
                {
                    Console.WriteLine((i + 1) + ") " + ShopInventory[i].WeaponData.Name);
                }
                Console.WriteLine();
                Console.WriteLine(player.Name + "'s funds: " + player.Moonbux + " Moonbux");
                Console.WriteLine("Select a number to view weapon stats. Type EXIT to leave the item shop: -");

                string itemChoice = Console.ReadLine();
                //First checks if player chooses to exit. Otherwise, it will check if player input is a valid ShopInventory index value.
                if (itemChoice.ToUpper() == "EXIT")
                {
                    return 9999;
                }
                else
                {
                    int intItemChoice;
                    isInt = int.TryParse(itemChoice, out intItemChoice);
                    if (isInt)
                    {
                        intItemChoice -= 1;
                        if (intItemChoice <= ShopInventory.Count && intItemChoice >= 0)
                        {
                            validInt = true;
                            return intItemChoice;
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. Please enter a valid item number.");
                            Console.ReadLine();
                            Console.Clear();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a valid item number.");
                        Console.ReadLine();
                        Console.Clear();
                    }
                }
            }
            Console.WriteLine("This should never trigger. Something has gone wrong.");
            return 666;
        }

        public void ExamineItem(int itemSelected, Soldier player, Inventory playerInventory)
        {
            Weapon selectedWeaponData = ShopInventory[itemSelected].WeaponData;

            Console.WriteLine("Weapon: " + selectedWeaponData.Name);
            Console.WriteLine("Damage: " + selectedWeaponData.DmgMin + "-" + selectedWeaponData.DmgMax);
            Console.WriteLine("Critical Hit Damage: " + selectedWeaponData.CritDmgMin + "-" + selectedWeaponData.CritDmgMax);
            Console.WriteLine("Critical Hit Chance: " + selectedWeaponData.CritChance + "%");
            Console.WriteLine(selectedWeaponData.Description + "\n");

            bool playerOwnsWeapon = CheckWeaponOwnership(selectedWeaponData, player, playerInventory);
            if (playerOwnsWeapon)
            {
                PurchaseOption(ShopInventory[itemSelected], player, playerInventory);
            }
            else
            {
                EquipOption(selectedWeaponData, player, playerInventory);
            }
        }

        public void EquipOption(Weapon selectedWeaponData, Soldier player, Inventory playerInventory)
        {
            bool SelectedWeaponAlreadyEquipped = CheckIfEquipped(selectedWeaponData, player);
            if (SelectedWeaponAlreadyEquipped)
            {
                Console.WriteLine("{0} already equipped. PRess any key to return to item list.");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("{0} already purchased and in storage.", selectedWeaponData.Name);
                Console.WriteLine("You currently have the {0} equipped.", player.WeaponEquipped.Name);
                Console.WriteLine("Press 1 to swap weapons. Press 2 to returen to item list.");
                string equipOrReturn = "";
                while (equipOrReturn == "")
                {
                    switch(equipOrReturn)
                    {
                        case "1":
                            {
                                playerInventory.Weapons.Add(player.WeaponEquipped);
                                player.WeaponEquipped = selectedWeaponData;
                                Console.WriteLine("{0} taken from inventory and equipped.", player.WeaponEquipped.Name);
                                break;

                            }
                        case "2":
                            break;
                        default:
                            {
                                Console.WriteLine("Invalid input. Press 1 to equip this weapon now. Press 2 to return to item list.");
                                break;
                            }
                    }
                }
            }
        }

        public static void RemoveWeaponFromInventory(Soldier player, Inventory playerInventory)
        {
            for (int i = 0; i < playerInventory.Weapons.Count; i++)
            {
                if (playerInventory.Weapons[i].Name == player.WeaponEquipped.Name)
                {
                    playerInventory.Weapons.RemoveAt(i);
                }
            }
        }

        public bool CheckIfEquipped(Weapon selectedWeaponData, Soldier player)
        {
            if (selectedWeaponData.Name == player.WeaponEquipped.Name)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void PurchaseOption(ShopItem selectedWeapon, Soldier player, Inventory playerInventory)
        {
            bool playerCanAffordItem = CheckFunds(selectedWeapon, player);
            if (playerCanAffordItem)
            {
                Console.WriteLine("Press 1 to buy weapon. Press 2 to return to item list.");
                string purchaseDecision = "";
                while (purchaseDecision == "")
                {
                    switch(purchaseDecision)
                    {
                        case "1":
                            {
                                player.Moonbux -= selectedWeapon.ItemPrice;
                                Console.WriteLine("{0} purchased! Press 1 to equip now. Press 2 to sent to inventory.", selectedWeapon.WeaponData.Name);
                                string purchaseItemDestination = "";
                                while (purchaseItemDestination == "")
                                {
                                    purchaseItemDestination = Console.ReadLine();
                                    switch(purchaseItemDestination)
                                    {
                                        case "1":
                                            {
                                                Console.WriteLine("{0} equipped. {1} sent to inventory", selectedWeapon.WeaponData.Name, player.WeaponEquipped.Name);
                                                playerInventory.Weapons.Add(player.WeaponEquipped);
                                                player.WeaponEquipped = selectedWeapon.WeaponData;
                                                break;
                                            }
                                        case "2":
                                            {
                                                playerInventory.Weapons.Add(selectedWeapon.WeaponData);
                                                Console.WriteLine("{0} sent to inventory. Press any key to continue", selectedWeapon.WeaponData.Name);
                                                Console.ReadLine();
                                                break;
                                            }
                                        default:
                                            {
                                                Console.WriteLine("Invalid input. Press 1 to equip {0} now. Press 2 to sent to inventory.", selectedWeapon.WeaponData.Name);
                                                break;
                                            }
                                    }
                                }

                                break;
                            }
                        case "2":
                            {
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
            else
            {
                Console.WriteLine("Insufficient funds! Press any key to return to item list.");
                Console.ReadLine();
            }
        }

        public bool CheckFunds(ShopItem selectedWeapon, Soldier player)
        {
            if (selectedWeapon.ItemPrice <= player.Moonbux)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckWeaponOwnership(Weapon selectedWeaponData, Soldier player, Inventory playerInventory)
        {
            if (selectedWeaponData.Name == player.WeaponEquipped.Name)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < playerInventory.Weapons.Count; i++)
                {
                    if (selectedWeaponData.Name == playerInventory.Weapons[i].Name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }

    //This class deals data handling; Saving and loading game states.
    static class State
    {
        //The following variable requires System.IO library to make Path.Combine work
        public static string SaveLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.xml");

        //The Save and Load functions requires System.Xml.Serialization library to work
        public static void Save(Soldier player)
        {
            using (Stream stream = new FileStream(SaveLocation, FileMode.Create))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Soldier));
                serializer.WriteObject(stream, player);
            }
        }

        public static Soldier Load(Soldier player)
        {
            DataContractSerializer Deserializer = new DataContractSerializer(typeof(Soldier));
            using (Stream stream = File.OpenRead(State.SaveLocation))
            {
                Soldier retrievedState = (Soldier)Deserializer.ReadObject(stream);
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
            Aim = 80;
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

