namespace WebApi.DataTransferObjects.Games.Responses
{
    public record GameArtworkSummary(
        string Type,
        string SmallImageUrl,
        string MediumImageUrl,
        string LargeImageUrl);
}