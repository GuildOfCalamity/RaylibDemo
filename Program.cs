using System;
using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib; // So you don't have to type "Raylib." for every method.

namespace RaylibDemo
{
    public class Program
    {
        static bool useRandomMovement = true;
        static int MaxParticles = 250;
        static int screenWidth = 1600;
        static int screenHeight = 700;
        static string currentTexture = "resources/splat.png";

        // Particle structure with basic data.
        struct Particle
        {
            public Vector2 position;
            public Color color;
            public float alpha;
            public float size;
            public float rotation;
            public bool active; // Used to activate/deactivate particle.
        }

        #region [Random Movement]
        static Vector2 currentPosition;
        static Vector2 targetPosition;
        static Random random = new Random();
        static float speed = 3.0f;
        static float maxOffset = 15.0f;
        static float amplitude = (float)screenHeight / 2.111f;
        static float frequency = 0.22f;
        static float phase = 0f;
        static int direction = 1; // 1 for moving right, -1 for moving left
        static int behavior = 0;
        static float gravity = 2.7f;
        static float maxX;
        static float maxY;
        #endregion

        /// <summary>
        /// https://github.com/ChrisDill/Raylib-cs
        /// https://github.com/raysan5/raylib/wiki
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            #region [Original Basic Demo]
            //Raylib.InitWindow(800, 480, "Hello World");
            //while (!Raylib.WindowShouldClose())
            //{
            //    Raylib.BeginDrawing();
            //    Raylib.ClearBackground(Color.WHITE);
            //
            //    Raylib.DrawText("Hello, world!", 12, 12, 20, Color.BLACK);
            //
            //    Raylib.EndDrawing();
            //}
            //Raylib.CloseWindow();
            #endregion

            //--------------------------------------------------------------------------------------
            // Initialization
            //--------------------------------------------------------------------------------------
            maxX = screenWidth;
            maxY = screenHeight;
            currentPosition = new Vector2(screenWidth/2, screenHeight/2);
            random = new Random();

            SetWindowState(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
            InitWindow(screenWidth, screenHeight, "Particles Blend Demo");
            SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);

            // Particles pool, reuse them!
            Particle[] mouseTail = new Particle[MaxParticles];

            // Initialize particles
            for (int i = 0; i < mouseTail.Length; i++)
            {
                mouseTail[i].position = new Vector2(0, 0);
                mouseTail[i].color = new Color(GetRandomValue(0, 255), GetRandomValue(0, 255), GetRandomValue(0, 255), 255);
                mouseTail[i].alpha = 0.9f;
                mouseTail[i].size = (float)GetRandomValue(1, 28) / 20.0f;
                mouseTail[i].rotation = GetRandomValue(0, 360);
                mouseTail[i].active = false;
            }

            Texture2D smoke = LoadTexture(currentTexture);
            BlendMode blending = BlendMode.BLEND_ALPHA;

            SetTargetFPS(60);

            // Main game loop
            while (!WindowShouldClose())
            {
                if (IsWindowResized())
                {
                    screenWidth = GetScreenWidth();
                    screenHeight = GetScreenHeight();
                    // For sinusoidal mode.
                    amplitude = (float)screenHeight / 2.111f;
                }
                //--------------------------------------------------------------------------------------
                // Update
                //----------------------------------------------------------------------------------

                // Activate one particle every frame and update active particles.
                // NOTE: Particles initial position should be mouse position when activated
                // NOTE: Particles fall down with gravity and rotation... and disappear after 2 seconds (alpha = 0)
                // NOTE: When a particle disappears, active = false and it can be reused.
                for (int i = 0; i < mouseTail.Length; i++)
                {
                    if (!mouseTail[i].active)
                    {
                        mouseTail[i].active = true;
                        mouseTail[i].alpha = 1.0f;
                        if (!useRandomMovement)
                            mouseTail[i].position = GetMousePosition();
                        else
                        {
                            switch (behavior)
                            {
                                case 0:
                                    mouseTail[i].position = GenerateNextSinusoidalPosition();
                                    break;
                                case 1:
                                    mouseTail[i].position = GenerateNextTargetPosition();
                                    break;
                                case 2:
                                    if (GetRandomValue(1, 11) > 5)
                                        mouseTail[i].position = GenerateNextPosition();
                                    else
                                        mouseTail[i].position = GenerateNextPerlinPosition(41);
                                    break;
                                default:
                                    mouseTail[i].position = GenerateNextSinusoidalPosition();
                                    break;
                            }
                        }
                        i = mouseTail.Length;
                    }
                }

                // Update the alphas and gravity for existing assets.
                for (int i = 0; i < mouseTail.Length; i++)
                {
                    if (mouseTail[i].active)
                    {
                        mouseTail[i].position.Y += gravity / 2;
                        mouseTail[i].alpha -= 0.005f;

                        if (mouseTail[i].alpha <= 0.0f)
                            mouseTail[i].active = false;

                        mouseTail[i].rotation += 2.0f;
                    }
                }

                #region [Keyboard Listerners]
                if (IsKeyPressed(KeyboardKey.KEY_SPACE))
                {
                    if (blending == BlendMode.BLEND_ALPHA)
                        blending = BlendMode.BLEND_ADDITIVE;
                    else
                        blending = BlendMode.BLEND_ALPHA;
                }

                if (IsKeyPressed(KeyboardKey.KEY_F))
                {
                    if (IsWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE))
                        ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                    else
                        SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                }

                if (IsKeyPressed(KeyboardKey.KEY_A))
                {
                    if (behavior < 2)
                        behavior++;
                    else
                        behavior = 0;
                }
                
                if (IsKeyPressed(KeyboardKey.KEY_I))
                {
                    if (currentTexture.Contains("/splat"))
                        currentTexture = "resources/orb_large.png";
                    else if (currentTexture.Contains("/orb_large"))
                        currentTexture = "resources/orb_tiny.png";
                    else if (currentTexture.Contains("/orb_tiny"))
                        currentTexture = "resources/pop.png";
                    else if (currentTexture.Contains("/pop"))
                        currentTexture = "resources/spot.png";
                    else if (currentTexture.Contains("/spot"))
                        currentTexture = "resources/star.png";
                    else if (currentTexture.Contains("/star"))
                        currentTexture = "resources/box.png";
                    else if (currentTexture.Contains("/box"))
                        currentTexture = "resources/point.png";
                    else if (currentTexture.Contains("/point"))
                        currentTexture = "resources/embed.png";
                    else
                        currentTexture = "resources/splat.png";

                    UnloadTexture(smoke);
                    smoke = LoadTexture(currentTexture);
                }
                #endregion

                //----------------------------------------------------------------------------------
                // Draw
                //----------------------------------------------------------------------------------
                BeginDrawing();
                ClearBackground(Color.BLACK);

                BeginBlendMode(blending);

                // Draw active particles
                for (int i = 0; i < mouseTail.Length; i++)
                {
                    if (mouseTail[i].active)
                    {
                        Rectangle source = new Rectangle(0, 0, smoke.Width, smoke.Height);
                        Rectangle dest = new Rectangle(mouseTail[i].position.X, mouseTail[i].position.Y, smoke.Width * mouseTail[i].size, smoke.Height * mouseTail[i].size);
                        Vector2 position = new Vector2(smoke.Width * mouseTail[i].size / 2, smoke.Height * mouseTail[i].size / 2);
                        Color color = ColorAlpha(mouseTail[i].color, mouseTail[i].alpha);
                        DrawTexturePro(smoke, source, dest, position, mouseTail[i].rotation, color);
                    }
                }

                EndBlendMode();

                DrawText("Press SPACE to change blending mode", 20, 20, 18, Color.DARKGRAY);

                if (blending == BlendMode.BLEND_ALPHA)
                    DrawText("ALPHA blending", 20, screenHeight - 40, 18, Color.DARKGRAY);
                else
                    DrawText("ADDITIVE blending", 20, screenHeight - 40, 18, Color.RAYWHITE);

                EndDrawing();
            }

            //----------------------------------------------------------------------------------
            // De-Initialization
            //--------------------------------------------------------------------------------------
            UnloadTexture(smoke);

            CloseWindow();
            //--------------------------------------------------------------------------------------
        }

        static Vector2 GenerateNextPosition()
        {
            // Calculate a small random offset within the specified maxOffset
            float offsetX = (float)(random.NextDouble() - 0.5) * 2 * maxOffset;
            float offsetY = (float)(random.NextDouble() - 0.5) * 2 * maxOffset;

            // Update the current position with the random offset
            currentPosition.X += offsetX * speed;
            currentPosition.Y += offsetY * speed;

            // Ensure the new position stays within a reasonable range (optional)
            ClampPosition();

            return currentPosition;
        }

        static Vector2 GenerateNextPerlinPosition(int seed, float factor = 0.05f)
        {
            float noiseX = (float)PerlinNoise(currentPosition.X * factor, currentPosition.Y * factor, seed);
            float noiseY = (float)PerlinNoise(currentPosition.Y * factor, currentPosition.X * factor, seed);

            // Scale the noise values and update the current position
            currentPosition.X += noiseX * speed * 25;
            //currentPosition.X = Math.Abs(currentPosition.X);
            currentPosition.Y += noiseY * speed * 25;
            //currentPosition.Y = Math.Abs(currentPosition.Y);

            // Ensure the new position stays within a reasonable range (optional)
            ClampPosition();

            return currentPosition;
        }

        static Vector2 GenerateNextSinusoidalPosition()
        {
            float time = (float)Environment.TickCount / 1000.0f; // Use time for continuous movement
            float displacement = amplitude * (float)Math.Sin(2 * Math.PI * frequency * time + phase);

            currentPosition.X += speed * direction;
            currentPosition.Y = amplitude + displacement;

            // Check if the rightmost boundary is reached
            if (currentPosition.X >= screenWidth || currentPosition.X <= 0)
            {
                direction *= -1; // Reverse the direction
            }

            // Ensure the new position stays within a reasonable range (optional)
            ClampPosition();

            return currentPosition;
        }

        static void ClampPosition()
        {
            // Example: Keep the position within a 2D area (adjust as needed)
            currentPosition.X = Math.Clamp(currentPosition.X, 1, screenWidth-1);
            currentPosition.Y = Math.Clamp(currentPosition.Y, 1, screenHeight-1);
        }

        // Perlin noise function
        static double PerlinNoise(float x, float y, int seed)
        {
            int n = (int)x + (int)y * 57 + seed * 131;
            n = (n << 13) ^ n;
            return (1.0 - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0);
        }

        static double GetRandomNumber(double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        static void GenerateRandomTargetPosition()
        {
            float randomX = 0f;
            float randomY = 0f;
            
            do
            {   // Ensure the new position is not too similar to the previous position.
                randomX = (float)GetRandomNumber(0, maxX);
                randomY = (float)GetRandomNumber(0, maxY);
            } while (Vector2.DistanceSquared(currentPosition, new Vector2(randomX, randomY)) < 10000.0f); // Adjust the threshold as needed

            targetPosition = new Vector2(randomX, randomY);
        }

        static Vector2 GenerateNextTargetPosition()
        {
            float distance = Vector2.Distance(currentPosition, targetPosition);

            // Move towards the target position
            currentPosition = Vector2.Lerp(currentPosition, targetPosition, (speed * 1.96f) / distance);

            var dist = Vector2.Distance(currentPosition, targetPosition);
            if (dist <= speed) // Check if the target is reached
            {
                GenerateRandomTargetPosition(); // Generate a new random target
            }

            return currentPosition;
        }
    }
}
