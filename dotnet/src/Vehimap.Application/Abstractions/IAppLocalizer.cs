// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;

namespace Vehimap.Application.Abstractions;

public interface IAppLocalizer
{
    CultureInfo Culture { get; }

    string GetString(string key);

    string Format(string key, params object?[] args);
}
