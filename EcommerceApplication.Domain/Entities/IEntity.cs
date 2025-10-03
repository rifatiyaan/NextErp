namespace EcommerceApplicationWeb.Domain.Entities
{
    public interface IEntity<T>
    {
        T Id { get; set; }
        // nullable for root entities

        string Title { get; set; }
        DateTime CreatedAt { get; set; }
        //DateTime? UpdatedAt { get; set; }
    }
}
