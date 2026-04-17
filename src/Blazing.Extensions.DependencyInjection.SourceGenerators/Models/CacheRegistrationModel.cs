// <copyright file="CacheRegistrationModel.cs" company="Blazing.Extensions.DependencyInjection">
//   Copyright (c) Blazing.Extensions.DependencyInjection. All rights reserved.
// </copyright>

namespace Blazing.Extensions.DependencyInjection.SourceGenerators.Models;

/// <summary>
/// Lightweight model capturing the data needed for source-generated caching
/// service registration and invalidator registration.
/// </summary>
/// <param name="InterfaceFullyQualifiedName">
/// The fully-qualified name of the primary interface implemented by the decorated class
/// (e.g. <c>MyApp.Services.IProductService</c>).
/// </param>
/// <param name="ImplementationFullyQualifiedName">
/// The fully-qualified concrete implementation type name
/// (e.g. <c>MyApp.Services.ProductService</c>).
/// </param>
/// <param name="DecoratorClassName">
/// The simple class name of the generated caching decorator
/// (e.g. <c>Blazing_CachingDecorator_ProductService</c>).
/// </param>
/// <param name="InvalidatorClassName">
/// The simple class name of the generated invalidator
/// (e.g. <c>Blazing_CacheInvalidator_ProductService</c>).
/// </param>
internal sealed record CacheRegistrationModel(
    string InterfaceFullyQualifiedName,
    string ImplementationFullyQualifiedName,
    string DecoratorClassName,
    string InvalidatorClassName);
