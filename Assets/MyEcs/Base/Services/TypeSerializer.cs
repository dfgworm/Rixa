using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using System.Reflection;


public static class TypeSerializer
{
    public static List<Type> ProduceIdList(Type interfaceType)
    {

        var idToType = new List<Type>();
#if UNITY_INCLUDE_TESTS //This annoying thing is required so that test assemblies don't break sorting order, so i can properly playtest in unity
        var testsHashSet = new HashSet<Type>();
#endif
        byte idCount = 0;
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
#if UNITY_INCLUDE_TESTS
            bool isTests = false;
            if (assembly.GetName().Name.StartsWith("Tests"))
                isTests = true;
#endif
            foreach (Type type in assembly.GetTypes())
                if (interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    idToType.Add(type);
                    idCount++;
#if UNITY_INCLUDE_TESTS
                    if (isTests)
                        testsHashSet.Add(type);
#endif
                    if (idCount >= 0b_1111_1111)
                        throw new Exception(interfaceType.Name + " class count exceeded maximum");
                }
        }
        idToType.Sort((x, y) =>
#if UNITY_INCLUDE_TESTS
                testsHashSet.Contains(x) & !testsHashSet.Contains(y) ? 1 : !testsHashSet.Contains(x) & testsHashSet.Contains(y) ? -1 :
#endif
                x.Name.CompareTo(y.Name));
        return idToType;
    }
}
