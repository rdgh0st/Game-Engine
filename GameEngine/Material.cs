using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Material
{
    public Effect effect;
    public Texture2D Texture;
    public float Shininess { get; set; }
    public int Passes {get{return effect.CurrentTechnique.Passes.Count; } }
    public Matrix World { get; set; }
    public Vector3 Diffuse { get; set; }
    public Vector3 Ambient { get; set; }
    public Vector3 Specular { get; set; }
    public Camera Camera { get; set; }
    public Light Light { get; set; }
    public int CurrentTechnique { get; set; }
    private string FileName;
    
    public Material(Matrix world, Camera camera, Light light, ContentManager content, 
        string filename, int currentTechnique, float shininess, Texture2D texture)
    {
        effect = content.Load<Effect>(filename);
        FileName = filename;
        World = world; 
        Camera = camera;
        Light = light;
        CurrentTechnique = currentTechnique;
        Shininess = shininess;
        Texture = texture;
        Diffuse = Color.Gray.ToVector3();
        Ambient = Color.Gray.ToVector3();
        Specular = Color.Gray.ToVector3();
    }
    public virtual void Apply(int currentPass)
    {
        if (FileName == "CookTorrance")
        {
            effect.CurrentTechnique = effect.Techniques[CurrentTechnique];
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["View"].SetValue(Camera.View);
            effect.Parameters["Projection"].SetValue(Camera.Projection);
            Matrix worldInverseTransposeMatrix = Matrix.Transpose(
                Matrix.Invert(World));
            effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);
            effect.Parameters["AmbientColor"].SetValue(Ambient);
            effect.Parameters["AmbientIntensity"].SetValue(0.5f);

            effect.Parameters["DiffuseColor"].SetValue(Diffuse);
            effect.Parameters["DiffuseIntensity"].SetValue(0.5f);

            effect.Parameters["LightPosition"].SetValue(Light.Transform.Position);
            effect.Parameters["CameraPosition"].SetValue(Camera.Transform.Position);
            effect.Parameters["SpecularColor"].SetValue(Specular);
            effect.Parameters["Roughness"].SetValue(0.1f);
            effect.Parameters["LightColor"].SetValue(Light.Ambient.ToVector4());
            effect.Parameters["F0"].SetValue(1);
            if (CurrentTechnique == 1)
            {
                effect.Parameters["decalMap"].SetValue(Texture);
            }
            effect.CurrentTechnique.Passes[currentPass].Apply();
        }
        else
        {
            effect.CurrentTechnique = effect.Techniques[CurrentTechnique];
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["View"].SetValue(Camera.View);
            effect.Parameters["Projection"].SetValue(Camera.Projection);
            effect.Parameters["LightPosition"].SetValue(Vector3.Backward * 10 +
                                                        Vector3.Right * 5);
            effect.Parameters["CameraPosition"].SetValue(Camera.Transform.Position);
            effect.Parameters["Shininess"].SetValue(Shininess);
            effect.Parameters["AmbientColor"].SetValue(Ambient);
            effect.Parameters["DiffuseColor"].SetValue(Diffuse);
            effect.Parameters["SpecularColor"].SetValue(Specular);
            effect.Parameters["DiffuseTexture"].SetValue(Texture);
            effect.CurrentTechnique.Passes[currentPass].Apply();
        }
    }
}