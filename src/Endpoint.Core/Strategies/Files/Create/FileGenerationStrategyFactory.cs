﻿using Endpoint.Core.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Endpoint.Core.Strategies.Files.Create
{

    public class FileGenerationStrategyFactory : IFileGenerationStrategyFactory
    {
        private readonly IEnumerable<IFileGenerationStrategy> _strategies;

        public FileGenerationStrategyFactory(
            IEnumerable<IFileGenerationStrategy> strategies
            )
        {
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        }

        public void CreateFor(FileModel model)
        {
            var strategy = _strategies.Where(x => x.CanHandle(model)).OrderByDescending(x => x.Order).FirstOrDefault();

            strategy.Create(model);
        }
    }
}
