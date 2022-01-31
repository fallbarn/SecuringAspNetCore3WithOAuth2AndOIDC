// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Marvin.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            { };


        // sle note: Configure a client. It must know about the client, thus is hard coded.
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
               new Client
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    RequirePkce = true,
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:44389/signin-oidc" // maps back to the Image Gallery MVP app.
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                } };
    }
}