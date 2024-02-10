using Vintagestory.API.Common;

[assembly: ModInfo(name: "Flower Farming", modID: "flowerfarming")]

namespace FlowerFarming;

public class Core : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass("FlowerFarming.ItemPlantableSeed", typeof(FFItemPlantableSeed));
        api.World.Logger.Event("started '{0}' mod", Mod.Info.Name);
    }
}