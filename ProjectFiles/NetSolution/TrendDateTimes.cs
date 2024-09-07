#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.SQLiteStore;
using FTOptix.Store;
#endregion

public class TrendDateTimes : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        trendMin = LogicObject.GetVariable("TrendMin");
        trendMax = LogicObject.GetVariable("TrendMax");
        updateTask = new PeriodicTask(UpdateTrendMinMax, 1000, LogicObject);
        updateTask.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    private void UpdateTrendMinMax()
    {
        // ----------------------------------------------------------------
        // This is a fake method to update the trend min and max values
        // it will emulate the MQTT subscriber updating the trend min and max values
        // ----------------------------------------------------------------

        // Get the current date and time
        var now = DateTime.Now;
        // Get the trend object from the current instance
        var trend = Owner.Get<Trend>("Trend");
        // Set the trend min and max values
        trend.XAxis.SnapPosition = SnapPosition.Right;
        trend.XAxis.Follow = true;
        trendMin.Value = now.AddMilliseconds(-1 * trend.XAxis.Window);
        trendMax.Value = now;
    }

    private IUAVariable trendMin;
    private IUAVariable trendMax;
    private PeriodicTask updateTask;
}
