﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceShooterV2
{
    internal class Bullet : GameObject
    {
        //Variables
        private readonly bool _owner; //true indiciates player owned and false indicates enemy owned
        private readonly int _dmg;

        //Public Procedures
        public Bullet(double widthByHeight, int height, byte texNum, int xVelocity, int yVelocity, Vector2 startingPos, bool owner, int dmg)
            : base(widthByHeight, height, texNum, xVelocity, yVelocity)
        {
            _dmg = dmg;
            _owner = owner;
            _position = new Vector2(startingPos.X, startingPos.Y - _height/2);
        }

        //Public Accessors
        public bool Owner
        {
            get { return _owner; }
        }

        public int xVel
        {
            get { return _xVelocity; }
            set { _xVelocity = value; }
        }

        public int yVel
        {
            get { return _yVelocity;}
            set { _yVelocity = value; }
        }

        public int Dmg
        {
            get { return _dmg; }
        }
    }
}
