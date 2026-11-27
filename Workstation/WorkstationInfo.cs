namespace WorkStationC_;

internal static class WorkstationInfo
{

    internal sealed record GenAiDeviceSettings(
        string DisplayDevice,
        string PipelineDevice,
        IReadOnlyDictionary<string, string> Properties)
    {
        public string CacheKey => Properties.Count == 0
            ? PipelineDevice
            : $"{PipelineDevice}|{string.Join(";", Properties.Select(property => $"{property.Key}={property.Value}"))}";
    }

    internal static GenAiDeviceSettings BuildGenAiDeviceSettings(string target)
    {
        string trimmed = target.Trim();
        if (!IsOpenVinoMultiTarget(trimmed))
        {
            return new GenAiDeviceSettings(trimmed, trimmed, new Dictionary<string, string>());
        }

        string devicePriorities = string.Join(
            ",",
            trimmed["MULTI:".Length..]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(device => !string.IsNullOrWhiteSpace(device))
            .Distinct(StringComparer.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(devicePriorities))
        {
            return new GenAiDeviceSettings(trimmed, trimmed, new Dictionary<string, string>());
        }

        return new GenAiDeviceSettings(
            DisplayDevice: trimmed,
            PipelineDevice: $"HETERO:{devicePriorities}",
            Properties: new Dictionary<string, string>
            {
                ["MODEL_DISTRIBUTION_POLICY"] = "PIPELINE_PARALLEL",
            });
    }

    internal static bool IsOpenVinoMultiTarget(string target)
    {
        return target.Trim().StartsWith("MULTI:", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsGpuCompileTarget(string target)
    {
        string trimmed = target.Trim();
        if (!trimmed.StartsWith("GPU", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains(':', StringComparison.Ordinal))
        {
            return false;
        }

        int dotCount = 0;
        foreach (char character in trimmed)
        {
            if (character == '.')
            {
                dotCount++;
            }
        }

        return dotCount <= 1;
    }

    internal static IReadOnlyList<string> BuildMultiCompileTargetChoices(IReadOnlyList<string> gpuTargets)
    {
        if (gpuTargets.Count < 2)
        {
            return Array.Empty<string>();
        }

        List<string> choices = new()
        {
            $"MULTI:{string.Join(",", gpuTargets)}",
        };

        for (int first = 0; first < gpuTargets.Count - 1; first++)
        {
            for (int second = first + 1; second < gpuTargets.Count; second++)
            {
                choices.Add($"MULTI:{gpuTargets[first]},{gpuTargets[second]}");
            }
        }

        return choices
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    internal static IReadOnlyList<string> BuildCompileTargetChoices(OpenVinoSharp.Core core, IReadOnlyList<string> devices)
    {
        List<string> gpuTargets = new();
        bool cpuAvailable = false;

        foreach (string rawDevice in devices)
        {
            string device = rawDevice.Trim();
            if (device.Equals("CPU", StringComparison.OrdinalIgnoreCase))
            {
                cpuAvailable = true;
                continue;
            }

            if (!IsGpuCompileTarget(device))
            {
                continue;
            }

            if (device.Contains('.', StringComparison.Ordinal))
            {
                gpuTargets.Add(device);
                continue;
            }

            IReadOnlyList<string> subDevices = GetOpenVinoSubDeviceTargets(core, device);
            if (subDevices.Count == 0)
            {
                gpuTargets.Add(device);
                continue;
            }

            gpuTargets.AddRange(subDevices.Where(IsGpuCompileTarget));
        }

        string[] distinctGpuTargets = gpuTargets
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        List<string> targets = new();
        if (distinctGpuTargets.Length > 0)
        {
            targets.AddRange(distinctGpuTargets);
            targets.AddRange(BuildMultiCompileTargetChoices(distinctGpuTargets));
        }
        else if (cpuAvailable)
        {
            targets.Add("CPU");
        }

        return targets
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    internal static IReadOnlyList<string> GetOpenVinoSubDeviceTargets(OpenVinoSharp.Core core, string device)
    {
        string raw;
        try
        {
            raw = core.GetProperty(device, OpenVinoSharp.PropertyKeys.AvailableDevices);
        }
        catch
        {
            return Array.Empty<string>();
        }

        return raw
            .Split(new[] { ',', ';', '[', ']', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(subDevice => NormalizeOpenVinoSubDeviceName(device, subDevice))
            .Where(subDevice => !string.IsNullOrWhiteSpace(subDevice))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    internal static string NormalizeOpenVinoSubDeviceName(string parentDevice, string subDevice)
    {
        if (subDevice.Contains('.', StringComparison.Ordinal) || subDevice.Contains(':', StringComparison.Ordinal))
        {
            return subDevice;
        }

        return $"{parentDevice}.{subDevice}";
    }

    // The GPU's full name and the memory OpenVINO can use on it, or null when the GPU plugin does not report them.
    internal static (string Name, ulong MemoryBytes)? DescribeGpu(OpenVinoSharp.Core core, string device)
    {
        try
        {
            string name = core.GetProperty(device, "FULL_DEVICE_NAME").Trim();
            ulong memory = ulong.TryParse(core.GetProperty(device, "GPU_DEVICE_TOTAL_MEM_SIZE").Trim(), out ulong bytes) ? bytes : 0;
            return (name.Length > 0 ? name : device, memory);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return null;
        }
    }
}
