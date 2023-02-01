// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Models.Syntax;
using Endpoint.Core.Services;
using System.Collections.Generic;
using System.Text;

namespace Endpoint.Core.Builders
{
    public class RuleForBuilder
    {

        private readonly Context _context;
        private bool _validator;
        private bool _notNull;
        private bool _notEmpty;
        private string _entity;

        public RuleForBuilder()
        {
            _notEmpty = false;
            _notNull = false;
            _validator = false;
        }

        public RuleForBuilder WithValidator(bool validator = true)
        {
            _validator = validator;
            return this;
        }

        public RuleForBuilder WithEntity(string entity)
        {
            _entity = entity;
            return this;
        }

        public RuleForBuilder WithNotEmpty(bool notEmpty = true)
        {
            _notEmpty = notEmpty;
            return this;
        }

        public RuleForBuilder WithNotNull(bool notNull = true)
        {
            _notNull = notNull;
            return this;
        }

        public string Build()
        {
            var content = new StringBuilder();

            content.Append($"RuleFor(request => request.{((SyntaxToken)_entity).PascalCase})");

            if (_notNull)
            {
                content.Append(".NotNull()");
            }

            if (_validator)
            {
                content.Append($".SetValidator(new {((SyntaxToken)_entity).PascalCase}Validator())");
            }

            content.Append(";");

            return content.ToString();
        }
    }
}

