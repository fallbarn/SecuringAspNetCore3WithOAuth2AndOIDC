﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Marvin.IDP
{
    public static class Config
    {

        // sle note: refers to the claims that are made available to the middleware.
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource(
                    "roles",
                    "Your role(s)",
                    new List<string>() { "role" }),
                new IdentityResource(
                    "country",
                    "The country you're living in",
                    new List<string>() { "country" }),
                new IdentityResource(
                    "subscriptionlevel",
                    "Your subscription level",
                    new List<string>() { "subscriptionlevel" })
            };


        // sle note: refers Restful API that the Client Application uses, in this case the ImageGalleryAPI
        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource(
                    "imagegalleryapi",
                    "Image Gallery API",
                    new List<string>() { "role" })
                {
                    ApiSecrets = { new Secret("apisecret".Sha256()) }
                }
            };


        // sle note: Hardcoded to the Client application! In this case the 'ImageGallery' MVC controller website
        public static IEnumerable<Client> Clients =>
            new Client[]
            {

                new Client
                {
                    IdentityTokenLifetime = 130,
                    AuthorizationCodeLifetime = 120,
                    AccessTokenLifetime = 120,
                    AccessTokenType = AccessTokenType.Reference,                   
                    AllowOfflineAccess = true, 
                    UpdateAccessTokenClaimsOnRefresh = true,
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    // sle note: points the the middleware 'signin' api interface with the 'Client Application'
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:44389/signin-oidc"
                    },
                    // sle note: points the the middleware 'signout' api interface with the 'Client Application'
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:44389/signout-callback-oidc"
                    },

                    // sle note: The middleware expects the following scopes must be matched by
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "imagegalleryapi",
                        "country",          
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                } };
    }
}