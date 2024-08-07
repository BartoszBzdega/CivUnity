using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingJob
{
    public BuildingJob(Image BuildingJobIcon,
        string BuildingJobName,
        float totalProductionNeeded,
        float overflowedProduction,
        ProductionCompleteDelegate OnProductionComplete,
        ProductionBonusDelegate ProductionBonusFunc
    )
    {
        if (OnProductionComplete == null)
            throw new UnityException();

        this.BuildingJobIcon = BuildingJobIcon;
        this.BuildingJobName = BuildingJobName;
        this.totalProductionNeeded = totalProductionNeeded;
        productionDone = overflowedProduction;
        this.OnProductionComplete = OnProductionComplete;
        this.ProductionBonusFunc = ProductionBonusFunc;
    }

    public float totalProductionNeeded;
    public float productionDone;

    public Image BuildingJobIcon;
    public string BuildingJobName;

    public delegate void ProductionCompleteDelegate();
    public event ProductionCompleteDelegate OnProductionComplete;

    public delegate float ProductionBonusDelegate();
    public ProductionBonusDelegate ProductionBonusFunc;

    public float DoWork(float rawProduction)
    {
        if (ProductionBonusFunc != null)
        {
            rawProduction *= ProductionBonusFunc();
        }

        productionDone += rawProduction;

        if (productionDone >= totalProductionNeeded)
        {
            OnProductionComplete();
        }

        return totalProductionNeeded - productionDone;
    }

}
