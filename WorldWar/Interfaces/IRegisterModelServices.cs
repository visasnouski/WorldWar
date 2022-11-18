// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using WorldWar.Abstractions.DTOs;

namespace WorldWar.Interfaces;

public interface IRegisterModelServices
{
	Task RegisterAsync(InputModel input, string baseUri);
}