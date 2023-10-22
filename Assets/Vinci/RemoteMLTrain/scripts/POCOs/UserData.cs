using UnityEngine;

[System.Serializable]
public class UserData
{
    public string pubkey;
    public string username;
    public string bio;
    public string image_url;
    public string player_data;

    public Sprite avatar;
}



#nullable enable

[System.Serializable]
public class UserUpdate
{
    public string? pubkey = null;
    public string? username = null;
    public string? bio = null;
    public string? image_url = null;
    public string? player_data = null;
}