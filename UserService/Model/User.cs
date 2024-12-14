namespace UserMicroservice.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public List<Guid> CreatedProductIds { get; set; } = new();
        public Guid? ProductCartId { get; set; } 

      
        public User(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }
    }
}
