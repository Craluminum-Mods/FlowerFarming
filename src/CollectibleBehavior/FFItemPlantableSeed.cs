using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace FlowerFarming;

public class FFItemPlantableSeed : ItemPlantableSeed
{
    public static string GetCodeWithoutFirstPart(ItemSlot itemslot)
    {
        return itemslot.Itemstack.Collectible.CodeEndWithoutParts(1);
    }

    public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
    {
        CollectibleBehaviorGroundStorable groundStorable = itemslot?.Itemstack?.Collectible?.GetBehavior<CollectibleBehaviorGroundStorable>();
        EnumHandling handling = EnumHandling.PassThrough;

        if (blockSel == null)
        {
            groundStorable?.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
            return;
        }

        BlockPos position = blockSel.Position;

        BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(position);
        if (blockEntity is not BlockEntityFarmland)
        {
            groundStorable?.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
            return;
        }

        Block block = byEntity.World.GetBlock(CodeWithPath("crop-" + GetCodeWithoutFirstPart(itemslot) + "-1"));
        if (block == null)
        {
            return;
        }

        IPlayer player = null;
        if (byEntity is EntityPlayer)
        {
            player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
        }

        bool num = ((BlockEntityFarmland)blockEntity).TryPlant(block);
        if (num)
        {
            byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), position.X, position.Y, position.Z, player);
            ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            if (player == null || player.WorldData?.CurrentGameMode != EnumGameMode.Creative)
            {
                itemslot.TakeOut(1);
                itemslot.MarkDirty();
            }
        }

        if (num)
        {
            handHandling = EnumHandHandling.PreventDefault;
        }
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        Block oldCropBlock = world.GetBlock(CodeWithPath("crop-" + inSlot.Itemstack.Collectible.LastCodePart() + "-1"));
        if (oldCropBlock != null)
        {
            return;
        }

        Block cropBlock = world.GetBlock(CodeWithPath("crop-" + GetCodeWithoutFirstPart(inSlot) + "-1"));
        if (cropBlock == null || cropBlock.CropProps == null)
        {
            return;
        }

        dsc.AppendLine(Lang.Get("soil-nutrition-requirement") + cropBlock.CropProps.RequiredNutrient);
        dsc.AppendLine(Lang.Get("soil-nutrition-consumption") + cropBlock.CropProps.NutrientConsumption);

        double totalDays = cropBlock.CropProps.TotalGrowthDays;
        if (totalDays > 0)
        {
            var defaultTimeInMonths = totalDays / 12;
            totalDays = defaultTimeInMonths * world.Calendar.DaysPerMonth;
        }
        else
        {
            totalDays = cropBlock.CropProps.TotalGrowthMonths * world.Calendar.DaysPerMonth;
        }

        totalDays /= api.World.Config.GetDecimal("cropGrowthRateMul", 1);

        dsc.AppendLine(Lang.Get("soil-growth-time") + " " + Lang.Get("count-days", Math.Round(totalDays, 1)));
        dsc.AppendLine(Lang.Get("crop-coldresistance", Math.Round(cropBlock.CropProps.ColdDamageBelow, 1)));
        dsc.AppendLine(Lang.Get("crop-heatresistance", Math.Round(cropBlock.CropProps.HeatDamageAbove, 1)));
    }
}