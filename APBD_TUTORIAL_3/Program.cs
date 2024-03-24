using System;
using System.Collections.Generic;
using System.Linq;

 

public class CapacityExceededException : Exception
{
    public CapacityExceededException(string message) : base(message) { }
    
    
}


public abstract class TransporterUnit
{
    public double LoadWeight { get; protected set; }
    public int UnitHeight { get; private set; }
    public double BaseWeight { get; private set; }
    public int UnitDepth { get; private set; }
    public string UnitID { get; private set; }
    public double MaximumLoad { get; private set; }

    protected TransporterUnit(int height, double baseWeight, int depth, string id, double maxLoad)
    {
        UnitHeight = height;
        BaseWeight = baseWeight;
        UnitDepth = depth;
        UnitID = id;
        MaximumLoad = maxLoad;
    }

    public abstract void Fill(double weight);
    public abstract void ClearLoad();
}

public interface IAlertingSystem
{
    void HazardAlert();
}
 

public class GasUnit : TransporterUnit, IAlertingSystem
{
    public double UnitPressure { get; private set; }

    public GasUnit(int height, double baseWeight, int depth, string id, double maxLoad, double pressure)
        : base(height, baseWeight, depth, id, maxLoad)
    {
        UnitPressure = pressure;
    }

    public override void Fill(double weight)
    {
        if (weight > MaximumLoad) throw new CapacityExceededException("Exceeds maximum load.");
        LoadWeight = weight;
    }

    public override void ClearLoad()
    {
        LoadWeight *= 0.05;
    }

    public void HazardAlert()
    {
        Console.WriteLine($"!!Gas unit {UnitID} in danger!!");
    }
}


public class FluidUnit : TransporterUnit, IAlertingSystem
{
    public bool Hazard { get; private set; }

    public FluidUnit(int height, double baseWeight, int depth, string id, double maxLoad, bool hazard)
        : base(height, baseWeight, depth, id, maxLoad)
    {
        Hazard = hazard;
    }

    public override void Fill(double weight)
    {
        double permissibleLoad = Hazard ? MaximumLoad * 0.5 : MaximumLoad * 0.9;
        if (weight > permissibleLoad) throw new CapacityExceededException("Exceeds permissible load.");
        LoadWeight = weight;
    }

    public override void ClearLoad()
    {
        LoadWeight = 0;
    }

    public void HazardAlert()
    {
        if (Hazard)
        {
            Console.WriteLine($"Warning: Hazardous material in unit {UnitID}");
        }
    }
}

public class RefrigeratorUnit : TransporterUnit
{
    public string StoredProduct { get; private set; }
    public double SetTemperature { get; private set; }

    public RefrigeratorUnit(int height, double baseWeight, int depth, string id, double maxLoad, string product, double temperature)
        : base(height, baseWeight, depth, id, maxLoad)
    {
        StoredProduct = product;
        SetTemperature = temperature;
    }

    public override void Fill(double weight)
    {
        if (weight > MaximumLoad) throw new CapacityExceededException("Exceeds the maximum load amount");
        LoadWeight = weight;
    }

    public override void ClearLoad()
    {
        LoadWeight = 0;
    }
}

public class Ship
{
    public List<TransporterUnit> Units { get; private set; } = new List<TransporterUnit>();
    public double SpeedLimit { get; private set; }
    public int Capacity { get; private set; }
    public double WeightLimit { get; private set; }

    public Ship(double speed, int capacity, double limit)
    {
        SpeedLimit = speed;
        Capacity = capacity;
        WeightLimit = limit;
    }

    public void AddUnit(TransporterUnit unit)
    {
        if (Units.Count >= Capacity) throw new Exception("Ship is at full capacity.");
        if (Units.Sum(u => u.LoadWeight + u.BaseWeight) / 1000 + (unit.LoadWeight + unit.BaseWeight) / 1000 > WeightLimit) throw new Exception("Ship is at full weight limit");

        Units.Add(unit);
    }

    public void RemoveUnit(string id)
    {
        var unit = Units.FirstOrDefault(u => u.UnitID == id);
        if (unit != null) Units.Remove(unit);
    }
}

class ShipManagementSystem
{
    static void Main(string[] args)
    {
        var fleet = new List<Ship>();
        var cargoUnits = new List<TransporterUnit>();
        string action = "";

        while (action.ToLower() != "exit")
        {
            Console.WriteLine("1 enter a new ship");
            Console.WriteLine("2 make a new cargo unit");
            Console.WriteLine("3 put cargo on a ship");
            Console.WriteLine("4 remove cargo from a ship");
            Console.WriteLine("5 display details about cargo");
            Console.WriteLine("exit closes the app");
            Console.Write("Choose one of the options: ");
            action = Console.ReadLine();

            switch (action)
            {
                case "1":
                    RegisterShip(fleet);
                    break;
                case "2":
                    CreateCargoUnit(cargoUnits);
                    break;
                case "3":
                    AssignCargoToVessel(fleet, cargoUnits);
                    break;
                case "4":
                    RemoveCargoFromVessel(fleet);
                    break;
                case "5":
                    DisplayFleetAndCargo(fleet);
                    break;
                default:
                    if (action.ToLower() != "exit")
                    {
                        Console.WriteLine("Unknown command.");
                    }

                    break;
            }
        }
    }

    static void RegisterShip(List<Ship> fleet)
    {
        Console.Write(" maximum speed: ");
        double speed = double.Parse(Console.ReadLine());
        Console.Write("cargo capacity: ");
        int capacity = int.Parse(Console.ReadLine());
        Console.Write("weight limit (tons): ");
        double limit = double.Parse(Console.ReadLine());

        var vessel = new Ship(speed, capacity, limit);
        fleet.Add(vessel);
        Console.WriteLine("ship registered successfully.");
    }

    static void CreateCargoUnit(List<TransporterUnit> cargoUnits)
    {
        Console.WriteLine("choose cargo type (1. Fluid, 2. Gas, 3. Cool): ");
        string choice = Console.ReadLine();
        int height, depth;
        double baseWeight, maxLoad;

        Console.Write("Enter unit height: ");
        height = int.Parse(Console.ReadLine());
        Console.Write("base weight: ");
        baseWeight = double.Parse(Console.ReadLine());
        Console.Write("unit depth: ");
        depth = int.Parse(Console.ReadLine());
        Console.Write("unit ID: ");
        string id = Console.ReadLine();
        Console.Write("unit's maximum load: ");
        maxLoad = double.Parse(Console.ReadLine());

        TransporterUnit unit = null;
        switch (choice)
        {
            //for fluids
            case "1": 
                Console.Write("Is the material hazardous? (true/false): ");
                bool hazardous = bool.Parse(Console.ReadLine());
                unit = new FluidUnit(height, baseWeight, depth, id, maxLoad, hazardous);
                break;
            //for gas
            case "2": 
                Console.Write("Enter unit pressure: ");
                double pressure = double.Parse(Console.ReadLine());
                unit = new GasUnit(height, baseWeight, depth, id, maxLoad, pressure);
                break;
            //for 
            case "3": 
                Console.Write("Enter stored product type: ");
                string product = Console.ReadLine();
                Console.Write("Enter temperature setting: ");
                double temperature = double.Parse(Console.ReadLine());
                unit = new RefrigeratorUnit(height, baseWeight, depth, id, maxLoad, product, temperature);
                break;
        }

        if (unit != null)
        {
            cargoUnits.Add(unit);
            Console.WriteLine("Cargo unit created.");
        }
        else
        {
            Console.WriteLine("Invalid choice of cargo unit.");
        }
    }

    static void AssignCargoToVessel(List<Ship> fleet, List<TransporterUnit> cargoUnits)
    {
        if (fleet.Count == 0 || cargoUnits.Count == 0)
        {
            Console.WriteLine("Operation not possible");
            return;
        }

        Console.WriteLine("Choose a ship:");
        for (int i = 0; i < fleet.Count; i++)
        {
            Console.WriteLine(
                $"{i + 1}. ship with speed limit {fleet[i].SpeedLimit} and capacity {fleet[i].Capacity}");
        }

        int vesselIndex = int.Parse(Console.ReadLine()) - 1;

        Console.WriteLine("Choose acargo unit:");
        for (int i = 0; i < cargoUnits.Count; i++)
        {
            Console.WriteLine($"{i + 1}. Cargo unit with ID {cargoUnits[i].UnitID}");
        }

        int unitIndex = int.Parse(Console.ReadLine()) - 1;

        try
        {
            fleet[vesselIndex].AddUnit(cargoUnits[unitIndex]);
            Console.WriteLine("Cargo unit assigned.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to assign cargo unit: {ex.Message}");
        }
    }

    static void RemoveCargoFromVessel(List<Ship> fleet
    )
    {
        if (fleet.Count == 0)
        {
            Console.WriteLine("no ships registered.");
            return;
        }

        Console.WriteLine("Choose a ship to remove");
        for (int i = 0; i < fleet.Count; i++)
        {
            Console.WriteLine(
                $"{i + 1}. ship with speed limit {fleet[i].SpeedLimit} and capacity for {fleet[i].Capacity} units");
        }

        int vesselIndex = int.Parse(Console.ReadLine()) - 1;

        if (fleet[vesselIndex].Units.Count == 0)
        {
            Console.WriteLine("This ship has no cargo units loaded.");
            return;
        }

        Console.Write("Choose cargo unit to remove ");
        string unitID = Console.ReadLine();

        try
        {
            fleet[vesselIndex].RemoveUnit(unitID);
            Console.WriteLine("Cargo unit removed from the ship successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to remove cargo unit from the ship: {ex.Message}");
        }
    }

    static void DisplayFleetAndCargo(List<Ship> fleet)
    {
        if (fleet.Count == 0)
        {
            Console.WriteLine("No ships are currently registered.");
            return;
        }

        foreach (var ship in fleet)
        {
            Console.WriteLine(
                $"\nship Details: Maxi speed: {ship.SpeedLimit} knots, Capacity: {ship.Capacity} units, Weight Limit: {ship.WeightLimit} tons");
            if (ship.Units.Count > 0)
            {
                Console.WriteLine("Loaded Cargo Units:");
                foreach (var unit in ship.Units)
                {
                    Console.WriteLine(
                        $"- ID: {unit.UnitID}, Load Weight: {unit.LoadWeight} kg, Type: {unit.GetType().Name.Replace("Unit", "")}");
                }
            }
            else
            {
                Console.WriteLine("No cargo units are loaded on this ship.");
            }
        }
    }
}