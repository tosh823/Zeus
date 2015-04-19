﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeus.Helpers;

namespace Zeus.Engine
{
    public class Sphere
    {

        public double botBoundary;
        public double topBoundary;
        public double longitude;
        public double latitude;
        public double delta;
        private int capacity;
        public double ne0;
        public double[] neGrid;
        public double nip0;
        public double[] nipGrid;
        public double nin0;
        public double[] ninGrid;
        public List<Element> aerosolElements;
        public Element activeElement;

        public Sphere(InputData data, Element active) {
            this.ne0 = data.ne0;
            this.nip0 = data.nip0;
            this.nin0 = data.nin0;
            this.delta = data.delta;
            this.botBoundary = data.botBoundary;
            this.topBoundary = data.topBoundary;
            this.aerosolElements = data.aerosols;
            this.activeElement = active;
            this.latitude = data.latitude;
            this.longitude = data.longitude;
            this.capacity = (int)((topBoundary - botBoundary) / delta);
            neGrid = new double[this.capacity];
            neGrid[0] = ne0;
            nipGrid = new double[this.capacity];
            nipGrid[0] = nip0;
            ninGrid = new double[this.capacity];
            ninGrid[0] = nin0;
        }

        public void changeCoordinates(double longitude, double latitude) {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public double n() {
            double height = 0;
            double result = neGrid[0] + nipGrid[0] + ninGrid[0];
            for (int i = 1; i < capacity; i++) {
                height = i * delta;
                neGrid[i] = ne(i, neGrid[i - 1], height);
                nipGrid[i] = niPositive(i, nipGrid[i - 1], height);
                ninGrid[i] = niNegative(i, ninGrid[i - 1], height);
                result += neGrid[i] + nipGrid[i] + ninGrid[i];
            }
            return result;
        }

        public double q(Element el, double height) {
            double n = el.getNForHeight(height);
            int wave = 0;
            double sum = 0;
            while (wave <= el.maxWave) {
                sum += Mathematical.photonFlux(el, wave, latitude, longitude, height) * el.getAtomCSForWave(wave);
            }
            return n*sum;
        }

        public double recombinate(Element el, double elN, double neN) {
            double result = 0;
            result = el.recombCoeff * elN / neN;
            return result;
        }

        public double stickTo(double neN, double height) {
            double result = 0;
            foreach (Element el in aerosolElements) {
                double b = Mathematical.beta(el.radius);
                result += b * neN * el.getNForHeight(height);
            }
            return result;
        }

        public double neutralize(double niP, double niN) {
            double result = 0;
            double gamma = 0.5; // Коэффициент нейтрализации
            result = gamma * niP * niN;
            return result;
        }

        // Концентрация электронов
        public double ne(int step, double nePrev, double height) {
            // Создание
            double creation = 0;
            double ionization = q(activeElement, height);
            creation += ionization;
            // Потери
            double loss = 0;
            double recombination = recombinate(activeElement, nePrev, neGrid[step - 1]);
            loss += recombination;
            double sticking = stickTo(neGrid[step - 1], height);
            loss += sticking;
            return (nePrev + (creation - loss)*Constants.dt);
        }

        // Концентрация положительных ионов
        public double niPositive(int step, double nipPrev, double height) {
            // Создание
            double creation = 0;
            double ionization = q(activeElement, height);
            creation += ionization;
            // Потери
            double loss = 0;
            double recombination = recombinate(activeElement, nipPrev, neGrid[step - 1]);
            loss += recombination;
            double neutralization = neutralize(nipGrid[step - 1], ninGrid[step - 1]);
            loss += neutralization;
            return (nipPrev + (creation - loss) * Constants.dt);
        }

        // Концентрация отрицательных ионов
        public double niNegative(int step, double ninPrev, double height) {
            // Создание
            double creation = 0;
            double sticking = stickTo(neGrid[step - 1], height);
            creation += sticking;
            // Потери
            double loss = 0;
            double neutralization = neutralize(nipGrid[step - 1], ninGrid[step - 1]);
            loss += neutralization;
            return (ninPrev + (creation - loss) * Constants.dt);
        }
    }
}
