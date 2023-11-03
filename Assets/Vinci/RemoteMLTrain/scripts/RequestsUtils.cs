using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class RequestsUtils
{

    public static (byte[], byte[]) ParseMultipartResponse(byte[] data)
    {
        byte[] modelData = null;
        byte[] metricsData = null;

        string dataString = System.Text.Encoding.UTF8.GetString(data);
        Debug.Log("dataString: " + dataString.Length);

        // Split the response into parts by boundary
        string[] parts = dataString.Split(new string[] { "--Boundary-" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            // Find the header and content parts
            string[] sections = part.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (sections.Length >= 2)
            {
                string header = sections[0];
                string content = sections[1];
                Debug.Log("header: " + header.ToString());
                // Check if the part is the model or the metrics
                if (header.Contains("filename=model"))
                {
                    
                    modelData = RetrieveData("model.onnx", content);
                    Debug.Log("modelData: " + modelData.Length);
                }
                else if (header.Contains("filename=metrics"))
                {
                    metricsData = RetrieveData("metrics.json", content);
                }
            }
        }

        return (modelData, metricsData);
    }

    private static void SaveFile(string filename, byte[] content)
    {
        // Remove any trailing boundary markers
        int endIndex = Array.IndexOf(content, (byte)'\r');
        if (endIndex > 0)
        {
            byte[] trimmedContent = new byte[endIndex];
            Array.Copy(content, trimmedContent, endIndex);
            content = trimmedContent;
        }

        // Save the file (you might want to change the path)
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, content);

        Debug.Log("Saved " + filename + " to " + path);
    }

    public static void ParseMultipartResponseNew(byte[] data)
    {
        byte[] modelData = null;
        byte[] metricsData = null;
        // Split the response into parts by boundary
        string boundaryString = "--Boundary-";
        byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes(boundaryString);

        int previousIndex = 0;
        while (true)
        {
            int startIndex = ByteArrayIndexOf(data, boundaryBytes, previousIndex);
            if (startIndex < 0) break;
            startIndex += boundaryBytes.Length;

            int endIndex = ByteArrayIndexOf(data, boundaryBytes, startIndex);
            if (endIndex < 0) endIndex = data.Length;

            byte[] part = new byte[endIndex - startIndex];
            Array.Copy(data, startIndex, part, 0, part.Length);

            // Find the header and content parts
            int splitIndex = ByteArrayIndexOf(part, new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' }, 0);
            if (splitIndex >= 0)
            {
                byte[] header = new byte[splitIndex];
                Array.Copy(part, header, splitIndex);

                byte[] content = new byte[part.Length - splitIndex - 4];
                Array.Copy(part, splitIndex + 4, content, 0, content.Length);

                // Check if the part is the model or the metrics
                string headerString = System.Text.Encoding.UTF8.GetString(header);
                if (headerString.Contains("filename=model"))
                {
                    SaveFile("model.onnx", content);
                }
                else if (headerString.Contains("filename=metrics"))
                {
                    SaveFile("metrics.json", content);
                }
            }

            previousIndex = endIndex;
        }
    }


    private static int ByteArrayIndexOf(byte[] array, byte[] pattern, int startIndex)
    {
        int firstMatch = -1;
        for (int i = startIndex; i < array.Length - pattern.Length + 1; ++i)
        {
            bool isMatch = true;
            for (int j = 0; j < pattern.Length; ++j)
            {
                if (array[i + j] != pattern[j])
                {
                    isMatch = false;
                    break;
                }
            }
            if (isMatch)
            {
                firstMatch = i;
                break;
            }
        }
        return firstMatch;
    }

    public static byte[] RetrieveData(string filename, string content)
    {
        // Remove any trailing boundary markers
        content = content.Split(new string[] { "\r\n--" }, StringSplitOptions.RemoveEmptyEntries)[0];


        return  System.Text.Encoding.UTF8.GetBytes(content);
    }

    public static IEnumerable<string> ParseMultipartFormData(byte[] data, string boundary)
    {
        var formDataSections = new List<string>();
        var boundaryBytes = Encoding.UTF8.GetBytes("--" + boundary);
        var startIndex = 0;

        while (startIndex < data.Length)
        {
            var endIndex = IndexOf(data, boundaryBytes, startIndex);
            if (endIndex == -1)
            {
                break;
            }

            var section = Encoding.UTF8.GetString(data, startIndex, endIndex - startIndex);
            formDataSections.Add(section);

            startIndex = endIndex + boundaryBytes.Length;
        }

        return formDataSections;
    }

    public static byte[] ExtractContent(string section)
    {
        var startIndex = section.IndexOf("\r\n\r\n") + 4;
        var endIndex = section.LastIndexOf("\r\n");
        var content = section.Substring(startIndex, endIndex - startIndex);
        return Encoding.UTF8.GetBytes(content);
    }

    public static int IndexOf(byte[] data, byte[] pattern, int startIndex)
    {
        for (int i = startIndex; i < data.Length; i++)
        {
            if (IsMatch(data, i, pattern))
            {
                return i;
            }
        }
        return -1;
    }

    public static bool IsMatch(byte[] data, int position, byte[] pattern)
    {
        if (position + pattern.Length > data.Length)
        {
            return false;
        }

        for (int i = 0; i < pattern.Length; i++)
        {
            if (data[position + i] != pattern[i])
            {
                return false;
            }
        }
        return true;
    }
}