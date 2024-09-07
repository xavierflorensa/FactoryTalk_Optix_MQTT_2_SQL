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
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using System.Linq;
#endregion

public class FakeMQTTLogic : BaseNetLogic
{
    public string message = "0";
     public string message1 = "0";
    
    public override async void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        insertFakeDataTask = new PeriodicTask(InsertFakeData, 1000, LogicObject);
        insertFakeDataTask.Start();
        // ----------------------------------------------------------------
        // MQTT subscriber from example in https://github.com/hivemq/hivemq-mqtt-client-dotnet
        // ----------------------------------------------------------------
        // Setup Client options and instantiate
        var options = new HiveMQClientOptionsBuilder().
                    WithBroker("broker.hivemq.com").
                    WithPort(1883).
                    WithUseTls(false).
                    Build();
        var client = new HiveMQClient(options);
        // Setup an application message handlers BEFORE subscribing to a topic
        client.OnMessageReceived += (sender, args) =>
        {
             Log.Info("Message Received: {}", args.PublishMessage.PayloadAsString);
             //message = args.PublishMessage.PayloadAsString;
             if (args.PublishMessage.Topic == "/my_topic")
             {
                message = args.PublishMessage.PayloadAsString;
             }
             if (args.PublishMessage.Topic == "/my_new_topic")
             {
                message1 = args.PublishMessage.PayloadAsString;
             }
            
        };
        // Connect to the MQTT broker
        var connectResult = await client.ConnectAsync().ConfigureAwait(false);
        // Configure the subscriptions we want and subscribe
        var builder = new SubscribeOptionsBuilder();
        /*builder.WithSubscription("/my_topic", QualityOfService.AtLeastOnceDelivery)
       .WithSubscription("/my_new_topic", QualityOfService.ExactlyOnceDelivery);
       */
        builder.WithSubscription("/my_topic", QualityOfService.ExactlyOnceDelivery)
        .WithSubscription("/my_new_topic", QualityOfService.ExactlyOnceDelivery);
       
        var subscribeOptions = builder.Build();
        var subscribeResult = await client.SubscribeAsync(subscribeOptions);
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        insertFakeDataTask?.Dispose();
    }

    private async void InsertFakeData()
    {    // List of the tables to be used by the random method
        var dataTables = new string[] { "Line1" };
        var random = new Random();
        // Get a random table name from the list
        var tableName = dataTables[0];

        // Get the Database object from the current project
        var myStore = Project.Current.Get<Store>("DataStores/EmbeddedDatabase");
        // Get the table object from the database
        var myTable = myStore.Tables.Get<Table>(tableName);
        // Prepare the header for the insert query (list of columns)
        string[] columns = { "Speed", "Counter", "Timestamp" };
        // Create the new object, a bidimensional array where the first element
        // is the number of rows to be added, the second one is the number
        // of columns to be added (same size of the columns array)
        var values = new object[1, 3];
        // Set some values for each column
        values[0, 0] = UInt32.Parse(message);
        values[0, 1] = UInt32.Parse(message1);
        values[0, 2] = DateTime.Now;
        // Perform the insert query
        myTable.Insert(columns, values);
        
        // Some log for users
        //Log.Info("InsertFakeData", tableName + " - Speed: " + values[0, 0] + " - Counter: " + values[0, 1]);
        

        

        

        

       
    }

    private PeriodicTask insertFakeDataTask;
}
