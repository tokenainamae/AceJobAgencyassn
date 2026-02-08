using System.Text.Json;

public class RecaptchaHelper
{
    public static async Task<bool> Verify(string token, string secretKey)
    {
        var client = new HttpClient();
        var response = await client.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
            null
        );

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RecaptchaResponse>(json);

        return result.success && result.score >= 0.5;
    }
}

public class RecaptchaResponse
{
    public bool success { get; set; }
    public float score { get; set; }
}
