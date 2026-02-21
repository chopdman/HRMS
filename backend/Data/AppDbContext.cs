using backend.Entities.Common;
using backend.Entities.Achievements;
using backend.Entities.Games;
using backend.Entities.Referrals;
using backend.Entities.Travels;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Travel> Travels => Set<Travel>();
        public DbSet<TravelAssignment> TravelAssignments => Set<TravelAssignment>();
        public DbSet<TravelDocument> TravelDocuments => Set<TravelDocument>();
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExpenseProof> ExpenseDocuments => Set<ExpenseProof>();
        public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<GameSlot> GameSlots => Set<GameSlot>();
        public DbSet<GameSlotRequest> GameSlotRequests => Set<GameSlotRequest>();
        public DbSet<GameSlotRequestParticipant> GameSlotRequestParticipants => Set<GameSlotRequestParticipant>();
        public DbSet<GameBooking> GameBookings => Set<GameBooking>();
        public DbSet<GameBookingParticipant> GameBookingParticipants => Set<GameBookingParticipant>();
        public DbSet<GameHistory> GameHistories => Set<GameHistory>();
        public DbSet<UserGameInterest> UserGameInterests => Set<UserGameInterest>();

        public DbSet<JobOpening> JobOpenings => Set<JobOpening>();
        public DbSet<JobShare> JobShares => Set<JobShare>();
        public DbSet<Referral> Referrals => Set<Referral>();
        public DbSet<ReferralStatusLog> ReferralStatusLogs => Set<ReferralStatusLog>();
        public DbSet<GlobalConfig> GlobalConfigs => Set<GlobalConfig>();
        public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
        public DbSet<AchievementPost> AchievementPosts => Set<AchievementPost>();
        public DbSet<PostComment> PostComments => Set<PostComment>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
        public DbSet<RemovedContent> RemovedContents => Set<RemovedContent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ConfigureAppModels();
        }


    }
}