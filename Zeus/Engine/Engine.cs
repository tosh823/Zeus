﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeus.Helpers;

namespace Zeus.Engine
{
    public enum ActiveElement
    {
        Nitrogen,
        Oxygen
    };

    public struct InputData
    {
        public double longitude;
        public double latitude;
        public double ne0;
        public double nip0;
        public double nin0;
        public double delta;
        public double botBoundary;
        public double topBoundary;
        public List<Element> aerosols;
    };

    public class Engine
    {
        private static Engine instance;
        public Sphere lowAtmosphere;
        private string inputFilename;

        private Engine() {
            
        }

        public static Engine Instance {
            get {
                if (instance == null) {
                    instance = new Engine();
                }
                return instance;
            }
        }

        public void initSphereWithInputFile(string path) {
            inputFilename = path;
            Element active = loadActiveElement(ActiveElement.Nitrogen);
            InputData data = JsonWrapper.parseInputData(path);
            lowAtmosphere = new Sphere(data, active);
        }

        public void setCoordinates(double longitude, double latitude) {
            lowAtmosphere.changeCoordinates(longitude, latitude);
        }

        // Загружаем основной элемент
        private Element loadActiveElement(ActiveElement el) {
            string path = Constants.appJsonPath + getFilenameForElement(el);
            Element activeElement = JsonWrapper.deserializeJsonToElement(path);
            return activeElement;
        }

        private string getFilenameForElement(ActiveElement el) {
            switch (el) {
                case ActiveElement.Nitrogen:
                    return "nitrogen.json";
                case ActiveElement.Oxygen:
                    return "oxygen.json";
                default:
                    return "nitrogen.json";
            }
        }
    }
}
