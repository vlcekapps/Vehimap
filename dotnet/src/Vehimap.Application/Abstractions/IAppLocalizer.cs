// SPDX-License-Identifier: GPL-3.0-or-later
namespace Vehimap.Application.Abstractions;

public interface IAppLocalizer
{
    string GetString(string key);

    string Format(string key, params object?[] args);
}
