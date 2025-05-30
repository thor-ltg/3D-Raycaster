using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Transactions;

namespace game
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D basictexture;
        Texture2D rayt;
        Random rng = new Random();
        MouseState mus = Mouse.GetState();
        MouseState gammaltmus = Mouse.GetState();
        SpriteFont Arial;
        Vector2 pos = new Vector2(100, 100);
        KeyboardState tangetbord = Keyboard.GetState();
        double playerdir = 180;
        Rectangle ball;
        Vector2 vball = new Vector2(250, 50);
        float speed = 2f;
        float[] dark = new float[60];
        Color[] raycolor = new Color[60];
        float[] angle = new float[60];
        Rectangle[] ray = new Rectangle[60];
        Vector2[] d = new Vector2[60];
        Vector2[] rayvec = new Vector2[60];
        List<Rectangle> objects = new List<Rectangle>();
        bool minimap = false;
        KeyboardState gammalttangentbord = Keyboard.GetState();
        int playerz = 0;
        int playerzv = 0;
        int mapsizex = 500;
        int mapsizey = 1500;
        Rectangle moving;
        int movingx = 2;
        int movingy = 2;
        int playdirz = 0;
        float pany;
        float panx;
        float zoom = 1;
        List<Color> wallcolors = new List<Color>();
        Vector2 oldmousepos;
        bool clicked;
        Rectangle LCSwallghost;
        List<Rectangle> flat = new List<Rectangle>();
        List<int> flatdistance = new List<int>();
        List<bool> shows = new List<bool>();
        List<int> rayhit = new List<int>();
        Rectangle showstop;
        List<Rectangle> minimapflat = new List<Rectangle>();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            flat.Add(new Rectangle(250, 900, 20, 20));
            shows.Add(false);
            flatdistance.Add(0);
            minimapflat.Add(new Rectangle(250, 900, 20, 20));
            rayhit.Add(0);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            basictexture = Content.Load<Texture2D>("boll");
            rayt = Content.Load<Texture2D>("ray");
            for (int i = 0; i < 60; i++)
            {
                ray[i] = new Rectangle(400, 240, 12, 13);
                rayvec[i] = pos;
            }
            ball = new Rectangle(250, 50, basictexture.Width, basictexture.Height);
            mus = Mouse.GetState();
            Arial = Content.Load<SpriteFont>("arial");
            tangetbord = Keyboard.GetState();
            gammaltmus = Mouse.GetState();
            // TODO: use this.Content to load your game content here
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            gammalttangentbord = tangetbord;
            gammaltmus = mus;
            mus = Mouse.GetState();
            tangetbord = Keyboard.GetState();
            ball.X = (int)vball.X;
            ball.Y = (int)vball.Y;
            for (int i = 0; i < minimapflat.Count(); i++)
            {
                minimapflat[i] = new Rectangle(minimapflat[i].X, minimapflat[i].Y, minimapflat[i].Width, minimapflat[i].Height);
                flat[i] = minimapflat[i];
                shows[i] = false;
                rayhit[i] = 0;
                flatdistance[i] = 0;
            }
            initializeray();
            shootrays();
            movement();
            movingparts();
            draw3d();
            base.Update(gameTime);
        }
        void initializeray()
        {
            for (int i = 0; i < 60; i++)
            {
                rayvec[i].X = vball.X;
                rayvec[i].Y = vball.Y;
            }
        }
        void shootrays()
        {
            for (int i = 0; i < 60; i++)
            {
                raycolor[i] = Color.White;
                d[i].X = 0;
                d[i].Y = 0;
                angle[i] = (i + 60 + (float)playerdir) * (float)(Math.PI / 180);
                float perpenangle = (60 + (float)playerdir) * (float)(Math.PI / 180);
                Vector2 dir = new Vector2((float)Math.Cos(angle[i]), (float)Math.Sin(angle[i]) * -1);
                Vector2 perpenddir = new Vector2((float)Math.Cos(perpenangle), (float)Math.Sin(perpenangle));
                while (rayvec[i].Y > 0 && rayvec[i].X > 0 && rayvec[i].X < mapsizex - rayt.Width && rayvec[i].Y < mapsizey - rayt.Width)
                {
                    rayvec[i] += dir*8;
                    ray[i].X = (int)rayvec[i].X;
                    ray[i].Y = (int)rayvec[i].Y;
                    d[i] += perpenddir*8;
                    if (moving.Contains(rayvec[i]))
                    {
                        while (moving.Contains(rayvec[i]))
                        {
                            rayvec[i] -= dir;
                            ray[i].X = (int)rayvec[i].X;
                            ray[i].Y = (int)rayvec[i].Y;
                            d[i] -= perpenddir;
                        }
                        rayvec[i] += dir;
                        break;
                    }
                    for (int j = 0; j < flat.Count(); j++)
                    {
                        if (flat[j].Contains(rayvec[i]) && !shows[j])
                        {
                            flatdistance[j] = (int)Math.Sqrt(d[i].X * d[i].X + d[i].Y * d[i].Y);
                            shows[j] = true;
                            rayhit[j] = i;
                            showstop = new Rectangle((int)rayvec[i].X, (int)rayvec[i].Y, 5, 5);
                        }
                    }
                    bool breakout = false;
                    for (int j = 0; j < objects.Count; j++)
                    {
                        if (objects[j].Contains(rayvec[i]))
                        {
                            raycolor[i] = wallcolors[j];
                            breakout = true;
                            break;
                        }
                    }
                    if (breakout)
                    {
                        break;
                    }
                }
            }
        }
        void draw3d()
        {
            for (int i = 59, t = 0; i >= 0; i--, t++)
            {
                float distance = (float)Math.Sqrt(d[i].X * d[i].X + d[i].Y * d[i].Y);
                if (distance == 0)
                {
                    distance++;
                }
                dark[i] = 500 / distance;
                dark[i] = 1;
                dark[i] += 0.01f;
                ray[i].Width = 32;
                ray[i].X = 25 + ray[i].Width * t;
                ray[i].Height = (int)(50000 / distance);
                ray[i].Y = ((1080 - ray[i].Height) / 2)-playdirz;
            }
            for (int i = 0; i < rayhit.Count(); i++)
            {
                if (flatdistance[i] == 0)
                {
                    flatdistance[i]++;
                }
                int h = 15000 / flatdistance[i] * 2;
                int x = ray[rayhit[i]].X;
                int w = h;
                int y = ((1080 - h) / 2) - playdirz;
                flat[i] = new Rectangle(x, y, w, h);
            }
        }
        void movement()
        {
            if (tangetbord.IsKeyDown(Keys.A))
            {
                if (!minimap || tangetbord.IsKeyDown(Keys.LeftShift))
                {
                    float angle = ((float)playerdir) * (float)(Math.PI / 180);
                    Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle) * -1);
                    Vector2 vballx = new Vector2(vball.X - dir.X * speed, vball.Y);
                    Vector2 vbally = new Vector2(vball.X, vball.Y - dir.Y * speed);
                    bool gohori = true;
                    bool govert = true;
                    Rectangle northwall = new Rectangle(0, 0, mapsizex + 20, 10);
                    Rectangle southwall = new Rectangle(0, mapsizey - 10, mapsizex, 10);
                    Rectangle eastwall = new Rectangle(0, 0, 10, mapsizey);
                    Rectangle westwall = new Rectangle(mapsizex - 10, 0, 10, mapsizey);
                    bool hitwallx = northwall.Contains(vballx) || southwall.Contains(vballx) || eastwall.Contains(vballx) || westwall.Contains(vballx);
                    bool hitwally = northwall.Contains(vbally) || southwall.Contains(vbally) || eastwall.Contains(vbally) || westwall.Contains(vbally);
                    if (hitwallx && hitwally)
                    {
                        gohori = false;
                        govert = false;
                    }
                    else if (hitwallx)
                    {
                        gohori = false;
                    }
                    else if (hitwally)
                    {
                        govert = false;
                    }

                    for (int i = 0; i < objects.Count(); i++)
                    {
                        if (!gohori && !govert) break;
                        bool hitobjectx = objects[i].Contains(vballx);
                        bool hitobjecty = objects[i].Contains(vbally);

                        if (hitobjectx && hitobjecty)
                        {
                            gohori = false;
                            govert = false;
                        }
                        else if (hitobjecty)
                        {
                            govert = false;

                        }
                        else if (hitobjectx)
                        {
                            gohori = false;

                        }
                    }
                    if (govert)
                    {
                        vball.Y -= dir.Y * speed;
                    }
                    if (gohori)
                    {
                        vball.X -= dir.X * speed;
                    }
                }
                else
                {
                    panx -= 10*(1/zoom);
                }
            }
            if (tangetbord.IsKeyDown(Keys.D))
            {
                if (!minimap || tangetbord.IsKeyDown(Keys.LeftShift))
                {
                    float angle = (float)(playerdir) * (float)(Math.PI / 180);
                    Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle) * -1);
                    Vector2 vballx = new Vector2(vball.X + dir.X * speed, vball.Y);
                    Vector2 vbally = new Vector2(vball.X, vball.Y + dir.Y * speed);
                    bool gohori = true;
                    bool govert = true;
                    Rectangle northwall = new Rectangle(0, 0, mapsizex + 20, 10);
                    Rectangle southwall = new Rectangle(0, mapsizey - 10, mapsizex, 10);
                    Rectangle eastwall = new Rectangle(0, 0, 10, mapsizey);
                    Rectangle westwall = new Rectangle(mapsizex - 10, 0, 10, mapsizey);
                    bool hitwallx = northwall.Contains(vballx) || southwall.Contains(vballx) || eastwall.Contains(vballx) || westwall.Contains(vballx);
                    bool hitwally = northwall.Contains(vbally) || southwall.Contains(vbally) || eastwall.Contains(vbally) || westwall.Contains(vbally);
                    if (hitwallx && hitwally)
                    {
                        gohori = false;
                        govert = false;
                    }
                    else if (hitwallx)
                    {
                        gohori = false;
                    }
                    else if (hitwally)
                    {
                        govert = false;
                    }

                    for (int i = 0; i < objects.Count(); i++)
                    {
                        if (!gohori && !govert) break;
                        bool hitobjectx = objects[i].Contains(vballx);
                        bool hitobjecty = objects[i].Contains(vbally);

                        if (hitobjectx && hitobjecty)
                        {
                            gohori = false;
                            govert = false;
                        }
                        else if (hitobjecty)
                        {
                            govert = false;
                        }
                        else if (hitobjectx)
                        {
                            gohori = false;
                        }
                    }
                    if (govert)
                    {
                        vball.Y += dir.Y * speed;
                    }
                    if (gohori)
                    {
                        vball.X += dir.X * speed;
                    }
                }
                else
                {
                    panx += 10*(1/zoom);
                }
            }
            if (tangetbord.IsKeyDown(Keys.W))
            {
                if (!minimap || tangetbord.IsKeyDown(Keys.LeftShift))
                {
                    float angle = (float)(playerdir+90) * (float)(Math.PI / 180);
                    Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle) * -1);
                    Vector2 vballx = new Vector2(vball.X + dir.X * speed, vball.Y);
                    Vector2 vbally = new Vector2(vball.X, vball.Y + dir.Y * speed);
                    bool gohori = true;
                    bool govert = true;
                    Rectangle northwall = new Rectangle(0, 0, mapsizex + 20, 10);
                    Rectangle southwall = new Rectangle(0, mapsizey - 10, mapsizex, 10);
                    Rectangle eastwall = new Rectangle(0, 0, 10, mapsizey);
                    Rectangle westwall = new Rectangle(mapsizex - 10, 0, 10, mapsizey);
                    bool hitwallx = northwall.Contains(vballx) || southwall.Contains(vballx) || eastwall.Contains(vballx) || westwall.Contains(vballx);
                    bool hitwally = northwall.Contains(vbally) || southwall.Contains(vbally) || eastwall.Contains(vbally) || westwall.Contains(vbally);
                    if (hitwallx && hitwally)
                    {
                        gohori = false;
                        govert = false;
                    }
                    else if (hitwallx)
                    {
                        gohori = false;
                    }
                    else if (hitwally)
                    {
                        govert = false;
                    }

                    for (int i = 0; i < objects.Count(); i++)
                    {
                        if (!gohori && !govert) break;
                        bool hitobjectx = objects[i].Contains(vballx);
                        bool hitobjecty = objects[i].Contains(vbally);

                        if (hitobjectx && hitobjecty)
                        {
                            gohori = false;
                            govert = false;
                        }
                        else if (hitobjecty)
                        {
                            govert = false;
                        }
                        else if (hitobjectx)
                        {
                            gohori = false;
                        }
                    }
                    if (govert)
                    {
                        vball.Y += dir.Y * speed;
                    }
                    if (gohori)
                    {
                        vball.X += dir.X * speed;
                    }
                }
                else
                {
                    pany -= 10 * (1 / zoom);
                }
            }
            if (tangetbord.IsKeyDown(Keys.S))
            {
                if (!minimap || tangetbord.IsKeyDown(Keys.LeftShift))
                {
                    float angle = ((float)playerdir + 90) * (float)(Math.PI / 180);
                    Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle) * -1);
                    Vector2 vballx = new Vector2(vball.X-dir.X*speed, vball.Y);
                    Vector2 vbally = new Vector2(vball.X, vball.Y-dir.Y*speed);
                    bool gohori = true;
                    bool govert = true;
                    Rectangle northwall = new Rectangle(0, 0, mapsizex+20, 10);
                    Rectangle southwall = new Rectangle(0, mapsizey-10, mapsizex, 10);
                    Rectangle eastwall = new Rectangle(0, 0, 10, mapsizey);
                    Rectangle westwall = new Rectangle(mapsizex-10, 0, 10, mapsizey);
                    bool hitwallx = northwall.Contains(vballx) || southwall.Contains(vballx) || eastwall.Contains(vballx) || westwall.Contains(vballx);
                    bool hitwally = northwall.Contains(vbally) || southwall.Contains(vbally) || eastwall.Contains(vbally) || westwall.Contains(vbally);
                    if (hitwallx && hitwally)
                    {
                        gohori = false;
                        govert = false;
                    }
                    else if (hitwallx)
                    {
                        gohori = false;
                    }
                    else if (hitwally)
                    {
                        govert = false;
                    }

                    for (int i = 0; i < objects.Count(); i++)
                    {
                        if (!gohori && !govert) break;
                        bool hitobjectx = objects[i].Contains(vballx);
                        bool hitobjecty = objects[i].Contains(vbally);

                        if (hitobjectx && hitobjecty)
                        {
                            gohori = false;
                            govert = false;
                        }
                        else if (hitobjecty)
                        {
                            govert = false;
                            
                        }
                        else if (hitobjectx)
                        {
                            gohori = false;
                            
                        }
                    }
                    if (govert)
                    {
                        vball.Y -= dir.Y * speed;
                    }
                    if (gohori)
                    {
                        vball.X -= dir.X * speed;
                    }
                }
                else
                {
                    pany += 10*(1/zoom);
                }
            }
            if (tangetbord.IsKeyDown(Keys.Left))
            {
                playerdir+=2;
            }
            if (tangetbord.IsKeyDown(Keys.Right))
            {
                playerdir-=2;
            }
            if (moving.Contains(vball))
            {
                vball.Y = mapsizey / 2;
                vball.X = mapsizex / 2;
            }
            if (tangetbord.IsKeyDown(Keys.M) && gammalttangentbord.IsKeyUp(Keys.M))
            {
                minimap = !minimap;
            }
            if (tangetbord.IsKeyDown(Keys.Space) && gammalttangentbord.IsKeyUp(Keys.Space) && playerz == 0)
            {
                playerzv = 100;
            }
            if (tangetbord.IsKeyDown(Keys.Y) && gammalttangentbord.IsKeyUp(Keys.Y))
            {
                mapsizey += 1000;
                byte redValue = (byte)rng.Next(255);
                byte greenValue = (byte)rng.Next(255);
                byte blueValue = (byte)rng.Next(255);
                Color rngcolor = new Color(redValue, greenValue, blueValue);
                Rectangle rectangle;
                rectangle = new Rectangle((mapsizex- rng.Next(500)), mapsizey - rng.Next(900), rng.Next(200), rng.Next(200));
                objects.Add(rectangle);
                wallcolors.Add(rngcolor);
                speed *= 1.25F;
            }
            if (tangetbord.IsKeyDown(Keys.X) && gammalttangentbord.IsKeyUp(Keys.X))
            {
                mapsizex += 1000;
                byte redValue = (byte)rng.Next(255);
                byte greenValue = (byte)rng.Next(255);
                byte blueValue = (byte)rng.Next(255);
                Color rngcolor = new Color(redValue, greenValue, blueValue);
                Rectangle rectangle;
                rectangle = new Rectangle(( mapsizex- rng.Next(500)), mapsizey - rng.Next(900), rng.Next(200), rng.Next(200));
                objects.Add(rectangle);
                wallcolors.Add(rngcolor);
                speed *= 1.25F;
            }
            playerz += playerzv;
            if (playerz <= 0)
            {
                playerzv = 0;
                playerz = 0;
            }
            else
            {
                playerzv -= 8;
            }
            playerzv /= 2;
            if (tangetbord.IsKeyDown(Keys.Down) && playdirz < 480)
            {
                playdirz+=24;
            }
            if (tangetbord.IsKeyDown(Keys.Up) && playdirz > -480)
            {
                playdirz-=24;
            }
            if (tangetbord.IsKeyDown(Keys.R))
            {
                panx = 0;
                pany = 0;
                zoom = 1;
            }
            if (tangetbord.IsKeyDown(Keys.O))
            {
                zoom /= 1.05f;
            }
            if (tangetbord.IsKeyDown(Keys.I))
            {
                zoom *= 1.05f;
            }
            if (clicked && minimap)
            {
                if (mus.Position.X < oldmousepos.X)
                {
                    if (mus.Position.Y < oldmousepos.Y)
                    {
                        LCSwallghost = new Rectangle(mus.Position.X, mus.Position.Y, (int)Math.Abs(mus.Position.X - oldmousepos.X), (int)Math.Abs(mus.Position.Y - oldmousepos.Y));
                    }
                    else
                    {
                        LCSwallghost = new Rectangle(mus.Position.X, (int)oldmousepos.Y, (int)Math.Abs(mus.Position.X - oldmousepos.X), (int)Math.Abs(mus.Position.Y - oldmousepos.Y));
                    }
                }
                else
                {
                    if (mus.Position.Y < oldmousepos.Y)
                    {
                        LCSwallghost = new Rectangle((int)oldmousepos.X, mus.Position.Y, (int)Math.Abs(mus.Position.X - oldmousepos.X), (int)Math.Abs(mus.Position.Y - oldmousepos.Y));
                    }
                    else
                    {
                        LCSwallghost = new Rectangle((int)oldmousepos.X, (int)oldmousepos.Y, (int)Math.Abs(mus.Position.X - oldmousepos.X), (int)Math.Abs(mus.Position.Y - oldmousepos.Y));
                    }
                }

                if (mus.LeftButton == ButtonState.Released && gammaltmus.LeftButton == ButtonState.Pressed)
                {
                    Rectangle objadd = new Rectangle((int)(LCSwallghost.X/zoom) + (int)panx, (int)(LCSwallghost.Y/zoom) + (int)pany, (int)(LCSwallghost.Width / zoom), (int)(LCSwallghost.Height / zoom));
                    objects.Add(objadd);
                    clicked = false;
                    wallcolors.Add(Color.White);
                }
            }
            else
            {
                if (mus.LeftButton == ButtonState.Released && gammaltmus.LeftButton == ButtonState.Pressed && minimap)
                {
                    LCSwallghost.X = mus.X;
                    LCSwallghost.Y = mus.Y;
                    oldmousepos.X = mus.X;
                    oldmousepos.Y = mus.Y;
                    clicked = true;
                }
            }
            if (tangetbord.IsKeyDown(Keys.P))
            {
                minimapflat[0] = new Rectangle(ball.X, ball.Y-40, 25, 25);
            }
        }
        void movingparts()
        {
            if (moving.X < 0 || moving.X > mapsizex - moving.Width)
            {
                movingx *= -1;
            }
            if (moving.Y < 0 || moving.Y > mapsizey - moving.Height)
            {
                movingy *= -1;
            }
            Rectangle movingafterx;
            Rectangle movingaftery;
            movingafterx = new Rectangle((int)(moving.X + movingx), moving.Y, moving.Width, moving.Height);
            movingaftery = new Rectangle(moving.X, (int)(moving.Y + movingy), moving.Width, moving.Height);
            for (int i = 0; i < objects.Count(); i++)
            {
                if (movingafterx.Intersects(objects[i]))
                {
                    movingx *= -1;
                }
                if (movingaftery.Intersects(objects[i]))
                {
                    movingy *= -1;
                }
            }
            moving.X += movingx;
            moving.Y += movingy;
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            if (minimap)
            {
                float x = vball.X;
                float y = vball.Y;
                Vector2 vb = new Vector2((x - panx) * zoom, (y - pany) * zoom);
                _spriteBatch.Draw(basictexture, vb, Color.White);
                for (int i = 0; i < 60; i++)
                {
                    float xr = rayvec[i].X;
                    float yr = rayvec[i].Y;
                    Vector2 rv = new Vector2((xr - panx) * zoom, (yr - pany) * zoom);
                    _spriteBatch.Draw(rayt, rv, raycolor[i]);
                }
                for (int i = 0; i < objects.Count; i++)
                {
                    Rectangle obj = new Rectangle((int)((objects[i].X - panx) * zoom), (int)((objects[i].Y - pany) * zoom), (int)(objects[i].Width * zoom), (int)(objects[i].Height * zoom));
                    _spriteBatch.Draw(basictexture, obj, wallcolors[i]);
                }
                Rectangle moveobj = new Rectangle((int)((moving.X - panx) * zoom), (int)((moving.Y - pany) * zoom), (int)(moving.Width * zoom), (int)(moving.Height * zoom));
                _spriteBatch.Draw(basictexture, moveobj, Color.LightSalmon);
                Rectangle northwall;
                Rectangle southwall;
                Rectangle eastwall;
                Rectangle westwall;
                northwall = new Rectangle((int)((0 - panx) * zoom), (int)((-5 - pany) * zoom), (int)(mapsizex * zoom) + 1, (int)(5 * zoom) + 1);
                southwall = new Rectangle((int)((0 - panx) * zoom), (int)((mapsizey - pany) * zoom), (int)(mapsizex * zoom) + 1, (int)(5 * zoom) + 1);
                eastwall = new Rectangle((int)((-5 - panx) * zoom), (int)((-5 - pany) * zoom), (int)(5 * zoom) + 1, (int)(mapsizey * zoom) + 1);
                westwall = new Rectangle((int)((mapsizex - panx) * zoom), (int)((-5 - pany) * zoom), (int)(5 * zoom) + 1, (int)(mapsizey * zoom) + 1);
                _spriteBatch.Draw(basictexture, northwall, Color.Gray);
                _spriteBatch.Draw(basictexture, southwall, Color.Gray);
                _spriteBatch.Draw(basictexture, eastwall, Color.DarkGray);
                _spriteBatch.Draw(basictexture, westwall, Color.DarkGray);
                for (int i = 0; i < minimapflat.Count(); i++)
                {
                _spriteBatch.Draw(basictexture, new Rectangle((int)(((minimapflat[i].X) - panx) * zoom), (int)((minimapflat[i].Y - pany) * zoom), (int)(minimapflat[i].Height * zoom), (int)(minimapflat[i].Width * zoom)), Color.White);
                }
                _spriteBatch.Draw(basictexture, showstop, Color.White);
            }
            else
            {
                
                for (int i = 0; i < 60; i++)
                {
                    _spriteBatch.Draw(basictexture, ray[i], raycolor[i] * dark[i]);
                }
                for (int i = 0; i < flat.Count(); i++)
                {
                        _spriteBatch.Draw(basictexture, flat[i], Color.White);
                }
                
                
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}