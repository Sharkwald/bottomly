using Bottomly.Models;
using Bottomly.Repositories;

namespace Bottomly.Commands;

public class AddMemberCommand(IMemberRepository repository) : ICommand
{
    private readonly IMemberRepository _repository = repository;

    public string GetPurpose() => "Persists a new member for karma tracking";

    public async Task ExecuteAsync(Member member) => await _repository.AddAsync(member);
}