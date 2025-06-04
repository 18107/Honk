using DV.Simulation.Controllers;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Honk
{
    internal class Honk
    {
        private static float lastValue;

        public static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (!Main.mod.Enabled) return;

            TrainCar thisCar = PlayerManager.Car;
            if (thisCar == null || !thisCar.IsLoco) return;

            HornControl control = thisCar.SimController.controlsOverrider.Horn;
            float hornValue = control.neutralAt0 ? control.Value : (Mathf.Abs(control.Value - 0.5f) * 2);

            if (hornValue < 0.1f && hornValue == lastValue) return;
            lastValue = hornValue;

            //Find connected cars
            List<TrainCar> cars;
            switch (Main.settings.hornControlType)
            {
                default:
                case Settings.Option.Single:
                    return;
                case Settings.Option.MU:
                    if (thisCar.muModule == null) return;
                    cars = new List<TrainCar>();
                    TrainCar currentCar = thisCar;
                    while (currentCar.muModule.ConnectedFront)
                    {
                        currentCar = currentCar.muModule.FrontCable.connectedTo.muModule.train;
                        cars.Add(currentCar);
                    }
                    currentCar = thisCar;
                    while (currentCar.muModule.ConnectedRear)
                    {
                        currentCar = currentCar.muModule.RearCable.connectedTo.muModule.train;
                        cars.Add(currentCar);
                    }
                    if (thisCar.muModule.UseWireless)
                    {
                        thisCar.muModule.RemoteChannel.devices.Do(device => { if (!cars.Contains(device.train)) cars.Add(device.train); });
                    }
                    break;
                case Settings.Option.Train:
                    cars = thisCar.trainset.cars.Where(car => car.IsLoco).ToList();
                    break;
                case Settings.Option.All:
                    cars = CarSpawner.Instance.AllLocos.Where(car => !CarSpawner.Instance.IsCarInPool(car)).ToList();
                    break;
            }
            if (cars == null) return;
            cars.Remove(thisCar);

            //Activate horn
            foreach (TrainCar car in cars)
            {
                //DE6 Slug has no horn but can be listed through MU cable
                car.SimController.controlsOverrider.Horn?.Set(hornValue);
            }
        }

        internal static void ResetAll()
        {
            CarSpawner.Instance.AllLocos.Where(car => !CarSpawner.Instance.IsCarInPool(car)).Do(car => car.SimController.controlsOverrider.Horn?.Set(0));
        }
    }
}
