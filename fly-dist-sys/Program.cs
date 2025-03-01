using fly_dist_sys._1_Echo;
using System;

// no ICU（International Components for Unicode）
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1", EnvironmentVariableTarget.Process);

await Echo.TestAsync();

//#if CHALLENGE1_ECHO //CHALLENGE1_Echo
//        Console.Error.WriteLine("This is AppA");
//#elif CHALLENGE2_Unique_ID_Generation
//        Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE3a_Single_Node_Broadcast
//        Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE3b_Multi_Node_Broadcast
//        Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE3c_Fault_Tolerant_Broadcast
//       Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE3d_Efficient_Broadcast_Part1
//       Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE3e_Efficient_Broadcast_Part2
//       Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE4_Grow_Only_Counter
//        Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE5a_Single_Node_Kafka_Style_Log
//       Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE5b_Multi_Node_Kafka_Style_Log
//       Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE5c_Efficient_Kafka_Style_Log
//      Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE6a_Single_Node_Totally_Available_Transactions
//    Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE6b_Totally_Available_Read_Uncommitted_Transactions
//      Console.Error.WriteLine("This is AppB");
//#elif CHALLENGE6c_Totally_Available_Read_Committed_Transactions
//     Console.Error.WriteLine("This is AppB");
//#else
//Console.Error.WriteLine("This is AppB");
//#endif