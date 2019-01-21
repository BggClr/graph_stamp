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
		public class Query : IRequest<Response>
		{
			public string Keyword { get; set; }
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
			public string Category { get; set; }
			public string Url { get; set; }
		}

		public class MappingProfile : Profile
		{
			public MappingProfile()
			{
				CreateMap<Repository, Model>()
					.ForMember(d => d.Category, o => o.MapFrom(s => ComponentNameConverter.GetCategory(s.Name)))
					.ForMember(d => d.Name, o => o.MapFrom(s => ComponentNameConverter.GetName(s.Name)));
			}
		}

		public class QueryHandler : IRequestHandler<Query, Response>
		{
			private readonly GithubAuthentication _githubAuthentication;
			private readonly IMapper _mapper;

			public QueryHandler(GithubAuthentication githubAuthentication, IMapper mapper)
			{
				_githubAuthentication = githubAuthentication;
				_mapper = mapper;
			}

			public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
			{
				var client = new RestClient("https://api.github.com/user/repos");
				var r = new RestRequest(Method.GET);
				r.AddHeader("Authorization", _githubAuthentication.GetAuthorizationHeader());
				var response = await client.ExecuteTaskAsync<List<Repository>>(r, cancellationToken);

				return new Response
				{
					Repos = response.Data
						.Where(p => Regex.IsMatch(p.Name, $"{Settings.ComponentPrefix}.*"))
						.Select(_mapper.Map<Model>)
						.Where(p => request.Category == null || p.Category.Equals(request.Category, StringComparison.InvariantCultureIgnoreCase))
						.Where(p => request.Keyword == null || p.Name.ToLowerInvariant().Contains(request.Keyword.ToLowerInvariant()))
				};
			}
		}
	}

}
