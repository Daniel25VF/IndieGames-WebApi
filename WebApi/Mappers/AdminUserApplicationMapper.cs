using Application.Abstractions.Common;
using Application.Abstractions.Storage;
using Application.Games.Catalog.Responses;
using Application.Games.Media.Responses;
using Application.Genres.Responses;
using Application.Users.Responses;
using Ardalis.Result;
using WebApi.Common;
using WebApi.DataTransferObjects.Games.Responses;
using WebApi.DataTransferObjects.Users.Responses;
using WebApi.DataTransferObjects.Genres.Responses;
using WebApi.DataTransferObjects.AdminUser.Responses;

namespace WebApi.Mappers
{
    public class AdminUserApplicationMapper(IS3Service s3Service) : SignedUrlMapper(s3Service), IAdminUserApplicationMapper
    {
        public async Task<PaginatedResponse<UserListItemAdminResponse>> MapToUserPaginatedResponseAsync(
            PaginatedApplicationResponse<ApplicationUserListItem> paginatedResponse,
            CancellationToken cancellationToken)
        {
            return await PaginatedResponse<UserListItemAdminResponse>.FromApplicationResponseAsync(
                paginatedResponse,
                source => MapToUserListItemResponse(source, cancellationToken),
                cancellationToken);
        }

        public PaginatedResponse<GameCollectionListItemAdminResponse> MapToGameCollectionPaginatedResponse(
            PaginatedApplicationResponse<ApplicationUserCollectionListItem> paginatedResponse)
        {
            return PaginatedResponse<GameCollectionListItemAdminResponse>.FromApplicationResponse(
                paginatedResponse,
                MapToGameCollectionListItemResponse);
        }

        public async Task<Result<GetUserCartResponse>> MapToGetUserCartResponse(
            Result<IReadOnlyCollection<ApplicationUserCartItem>> result,
            CancellationToken cancellationToken)
        {
            return await result.MapAsync(source => MapToGetUserCartResponse(source, cancellationToken));
        }

        public async Task<GetUserCartResponse> MapToGetUserCartResponse(
            IReadOnlyCollection<ApplicationUserCartItem> cartItems,
            CancellationToken cancellationToken)
        {
            var mappedCartItems = await Task.WhenAll(
                cartItems.Select(cartItem => MapToGameSummaryResponse(cartItem.Game, cancellationToken)));

            return new GetUserCartResponse(mappedCartItems.ToList());
        }

        public async Task<Result<UserAdminResponse>> MapToUserResponse(Result<ApplicationUser> result, CancellationToken cancellationToken)
        {
            return await result.MapAsync(source => MapToUserResponse(source, cancellationToken));
        }

        public async Task<UserAdminResponse> MapToUserResponse(ApplicationUser applicationUser, CancellationToken cancellationToken)
        {
            var profilePicture = await MapToUserProfilePictureResponse(applicationUser, cancellationToken);

            return new UserAdminResponse(
                applicationUser.IdentityId,
                applicationUser.Username,
                applicationUser.DisplayUsername,
                profilePicture,
                applicationUser.CreatedGames.Count,
                applicationUser.OwnedGames.Count,
                applicationUser.CreatedAt,
                applicationUser.UpdatedAt);
        }

        public UpdateUserAdminResponse MapToUpdateUserResponse(ApplicationUserMutation applicationUser)
        {
            return new UpdateUserAdminResponse(
                applicationUser.IdentityId,
                applicationUser.Username,
                applicationUser.DisplayUsername,
                applicationUser.UpdatedAt);
        }

        public async Task<UserListItemAdminResponse> MapToUserListItemResponse(ApplicationUserListItem applicationUser, CancellationToken cancellationToken)
        {
            var profilePicture = await MapToUserProfilePictureResponse(applicationUser, cancellationToken);

            return new UserListItemAdminResponse(
                applicationUser.IdentityId,
                applicationUser.Username,
                applicationUser.DisplayUsername,
                profilePicture,
                applicationUser.CreatedGamesCount,
                applicationUser.OwnedGamesCount,
                applicationUser.CreatedAt,
                applicationUser.UpdatedAt);
        }

        public GameCollectionAdminResponse MapToGameCollectionResponse(ApplicationUserCollectionListItem gameCollection)
        {
            return new GameCollectionAdminResponse(
                gameCollection.Id,
                gameCollection.Name);
        }

        public GameCollectionListItemAdminResponse MapToGameCollectionListItemResponse(ApplicationUserCollectionListItem gameCollection)
        {
            return new GameCollectionListItemAdminResponse(
                gameCollection.Id,
                gameCollection.Name,
                gameCollection.GamesCount,
                gameCollection.PreviewSmallPictureUrls,
                gameCollection.CreatedAt,
                gameCollection.UpdatedAt);
        }

        public async Task<GameCollectionDetailsAdminResponse> MapToGameCollectionDetailsResponse(ApplicationUserCollectionDetails gameCollection, CancellationToken cancellationToken)
        {
            var games = await Task.WhenAll(
                gameCollection.Games.Select(game => MapToGameListItemResponse(game, cancellationToken)));

            return new GameCollectionDetailsAdminResponse(
                gameCollection.Id,
                gameCollection.Name,
                games.ToList(),
                gameCollection.CreatedAt,
                gameCollection.UpdatedAt);
        }

        async Task<GameSummary> MapToGameSummaryResponse(ApplicationUserGame applicationGame, CancellationToken cancellationToken)
        {
            var artworks = await Task.WhenAll(
                applicationGame.Artworks.Select(artwork => MapToGameArtworkSummaryResponse(artwork, cancellationToken)));

            var storePictures = await Task.WhenAll(
                applicationGame.Pictures.Select(picture => MapToGameStorePictureSummaryResponse(picture, cancellationToken)));

            return new GameSummary(
                applicationGame.Id,
                applicationGame.Title,
                applicationGame.Description,
                applicationGame.Genres.Select(MapToGenreSummaryResponse).ToList(),
                storePictures.ToList(),
                artworks.ToList());
        }

        GenreSummary MapToGenreSummaryResponse(ApplicationGenre applicationGenre)
        {
            return new GenreSummary(
                applicationGenre.Id,
                applicationGenre.Name);
        }

        async Task<GameArtworkSummary> MapToGameArtworkSummaryResponse(ApplicationGameArtwork applicationGameArtwork, CancellationToken cancellationToken)
        {
            var urls = await CreatePictureSignedUrlsAsync(
                applicationGameArtwork.SmallArtworkKey,
                applicationGameArtwork.MediumArtworkKey,
                applicationGameArtwork.LargeArtworkKey,
                cancellationToken);

            return new GameArtworkSummary(
                applicationGameArtwork.Type.ToString(),
                urls.SmallUrl,
                urls.MediumUrl,
                urls.LargeUrl);
        }

        async Task<GameStorePictureSummary> MapToGameStorePictureSummaryResponse(ApplicationGamePicture gameStorePicture, CancellationToken cancellationToken)
        {
            var urls = await CreatePictureSignedUrlsAsync(
                gameStorePicture.SmallPictureKey,
                gameStorePicture.MediumPictureKey,
                gameStorePicture.LargePictureKey,
                cancellationToken);

            return new GameStorePictureSummary(
                urls.SmallUrl,
                urls.MediumUrl,
                urls.LargeUrl);
        }

        async Task<GameListItemResponse> MapToGameListItemResponse(ApplicationGame game, CancellationToken cancellationToken)
        {
            var artworks = await Task.WhenAll(
                game.Artworks.Select(artwork => MapToGameArtworkSummaryResponse(artwork, cancellationToken)));

            return new GameListItemResponse(
                game.Id,
                game.Title,
                game.Price,
                game.Discount,
                artworks);
        }

        async Task<UserProfilePictureAdminResponse> MapToUserProfilePictureResponse(ApplicationUser applicationUser, CancellationToken cancellationToken)
        {
            var urls = await CreatePictureSignedUrlsAsync(
                applicationUser.ProfilePicture.SmallPictureKey,
                applicationUser.ProfilePicture.MediumPictureKey,
                applicationUser.ProfilePicture.LargePictureKey,
                cancellationToken);

            return new UserProfilePictureAdminResponse(
                urls.SmallUrl,
                urls.MediumUrl,
                urls.LargeUrl);
        }

        async Task<UserProfilePictureAdminResponse> MapToUserProfilePictureResponse(ApplicationUserListItem applicationUser, CancellationToken cancellationToken)
        {
            var urls = await CreatePictureSignedUrlsAsync(
                applicationUser.ProfilePicture.SmallPictureKey,
                applicationUser.ProfilePicture.MediumPictureKey,
                applicationUser.ProfilePicture.LargePictureKey,
                cancellationToken);

            return new UserProfilePictureAdminResponse(
                urls.SmallUrl,
                urls.MediumUrl,
                urls.LargeUrl);
        }
    }
}
