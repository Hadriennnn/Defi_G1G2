using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPathfinder
{
    Task<bool> CheckAllPathsAsync();
}