using fly_dist_sys._1_Echo;
using fly_dist_sys._2_Unique_ID_Generation;
using System;

// no ICU（International Components for Unicode）
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "1", EnvironmentVariableTarget.Process);

#if Challenge_1
  await Challenge_1.TestAsync();
#elif Challenge_2
  await Challenge_2.TestAsync();
#elif Challenge_3a
  await Challenge_3a.TestAsync();
#elif Challenge_3b
  await Challenge_3b.TestAsync();
#elif Challenge_3c
  await Challenge_3c.TestAsync();
#elif Challenge_3d
  await Challenge_3d.TestAsync();
#elif Challenge_3e
  await Challenge_3e.TestAsync();
#elif Challenge_4
  await Challenge_4.TestAsync();
#elif Challenge_5a
  await Challenge_5a.TestAsync();
#elif Challenge_5b
  await Challenge_5b.TestAsync();
#elif Challenge_5c
  await Challenge_5c.TestAsync();
#elif Challenge_6a
  await Challenge_6a.TestAsync();
#elif Challenge_6b
  await Challenge_6b.TestAsync();
#elif Challenge_6c
  await Challenge_6c.TestAsync();
#else
  Console.Error.WriteLine("Wrong Challenge Constant");
#endif