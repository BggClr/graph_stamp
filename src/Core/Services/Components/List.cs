using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Models;
using RestSharp;

namespace Stamp.Services.Components
{
	public class List
	{
		private const string stampComponentTopic = "stamp-component";
		public class Query : IRequest<Response>
		{
			public string Keyword { get; set; }
			public string Owner { get; set; }
			public string Category { get; set; }
		}

		public class Response
		{
			public IEnumerable<Model> Repos { get; set; }
		}

		public class Model
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string Owner { get; set; }
			public IList<string> Categories { get; set; }
			public string Url { get; set; }
		}

		public class MappingProfile : Profile
		{
			public MappingProfile()
			{
				CreateMap<Repository, Model>()
					.ForMember(d => d.Owner, o => o.MapFrom(s => s.Owner.Login))
					.ForMember(d => d.Categories, o => o.MapFrom(s => s.Topics.Where(p => !p.Equals(stampComponentTopic, StringComparison.InvariantCultureIgnoreCase))));
			}
		}

		private class RepositorySearchResult
		{
			public IList<Repository> Items { get; set; }
		}

		public class QueryHandler : IRequestHandler<Query, Response>
		{
			private readonly IMapper _mapper;

			public QueryHandler(IMapper mapper)
			{
				_mapper = mapper;
			}

			public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
			{
				var client = new RestClient($"https://api.github.com/search/repositories?q={GetSearchQuery(request)}");
				var r = new RestRequest(Method.GET);
				r.AddHeader("Accept", "application/vnd.github.mercy-preview+json");
				var response = await client.ExecuteTaskAsync<RepositorySearchResult>(r, cancellationToken);

				return new Response
				{
					Repos = response.Data.Items
						.Select(_mapper.Map<Model>)
						.Where(p => request.Keyword == null || p.Name.ToLowerInvariant().Contains(request.Keyword.ToLowerInvariant()))
				};
			}

			private string GetSearchQuery(Query request)
			{
				var keywordQuery = new[] {request.Keyword}.Where(p => !string.IsNullOrWhiteSpace(p));

				var facetsQuery = new[]
				{
					(SerchKey: "topic", Value: stampComponentTopic),
					(SerchKey: "topic", Value: request.Category),
					(SerchKey: "user", Value: request.Owner)
				}
					.Where(p => !string.IsNullOrWhiteSpace(p.Value))
					.Select(p => $"{p.SerchKey}:{p.Value}");

				return string.Join("+", keywordQuery.Union(facetsQuery));
			}
		}
	}

}
