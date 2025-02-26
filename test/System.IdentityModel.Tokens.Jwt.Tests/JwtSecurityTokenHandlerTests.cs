//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.TestUtils;
using Microsoft.IdentityModel.Tokens;
using Xunit;

#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant

namespace System.IdentityModel.Tokens.Jwt.Tests
{
    public class JwtSecurityTokenHandlerTests
    {
        [Theory, MemberData(nameof(ActorTheoryData))]
        public void Actor(JwtTheoryData theoryData)
        {
            var context = new CompareContext();
            try
            {
                var claimsIdentity = theoryData.TokenHandler.ValidateToken(theoryData.Token, theoryData.ValidationParameters, out SecurityToken validatedToken).Identity as ClaimsIdentity;
                Assert.True(claimsIdentity.Actor != null);
                theoryData.ExpectedException.ProcessNoException(context);
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex, context.Diffs);
            }

            TestUtilities.AssertFailIfErrors(context);
        }

        public static TheoryData<JwtTheoryData> ActorTheoryData
        {
            get
            {
                var theoryData = new TheoryData<JwtTheoryData>();
                var handler = new JwtSecurityTokenHandler();

                // Actor validation is true
                // Actor will be validated using validationParameters since validationsParameters.ActorValidationParameters is null
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(ClaimSets.DefaultClaimsIdentity);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Actor, Default.AsymmetricJwt));
                var validationParameters = Default.AsymmetricSignTokenValidationParameters;
                validationParameters.ValidateActor = true;
                theoryData.Add(
                    new JwtTheoryData
                    { 
                        TestId = "ActorValidationUsingTVP - True",
                        ExpectedException = ExpectedException.NoExceptionExpected,
                        Token = handler.CreateEncodedJwt(Default.Issuer, Default.Audience, claimsIdentity, null, null, null, Default.AsymmetricSigningCredentials),
                        TokenHandler = handler,
                        ValidationParameters = validationParameters
                    }
                );

                // Actor validation is true
                // Actor is signed with symmetric key
                // TokenValidationParameters.ActorValidationParameters will not find signing key because an assymetric signing key is provided
                claimsIdentity = new ClaimsIdentity(ClaimSets.DefaultClaimsIdentity);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Actor, Default.SymmetricJws));
                validationParameters = Default.AsymmetricSignTokenValidationParameters;
                validationParameters.ValidateActor = true;
                validationParameters.ActorValidationParameters = Default.AsymmetricSignTokenValidationParameters;
                theoryData.Add(
                    new JwtTheoryData
                    {
                        TestId = "ActorValidationUsingActorTVP - ExceptionExpected",
                        ExpectedException = ExpectedException.SecurityTokenSignatureKeyNotFoundException("IDX10501"),
                        Token = handler.CreateEncodedJwt(Default.Issuer, Default.Audience, claimsIdentity, null, null, null, Default.AsymmetricSigningCredentials),
                        TokenHandler = handler,
                        ValidationParameters = validationParameters
                    }
                );

                // Actor validation is true
                // Actor is signed with symmetric key
                // TokenValidationParameters.ActorValidationParameters is null
                // TokenValidationParameters will be used, but will not find signing key because an assymetric signing key is provided
                claimsIdentity = new ClaimsIdentity(ClaimSets.DefaultClaimsIdentity);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Actor, Default.SymmetricJws));
                validationParameters = Default.AsymmetricSignTokenValidationParameters;
                validationParameters.ValidateActor = true;
                theoryData.Add(
                    new JwtTheoryData
                    {
                        TestId = "ActorValidationUsingTVP - ExceptionExpected",
                        ExpectedException = ExpectedException.SecurityTokenSignatureKeyNotFoundException("IDX10501"),
                        Token = handler.CreateEncodedJwt(Default.Issuer, Default.Audience, claimsIdentity, null, null, null, Default.AsymmetricSigningCredentials),
                        TokenHandler = handler,
                        ValidationParameters = validationParameters
                    }
                );


                // Actor validation is false
                // Actor is signed with symmetric key
                // TokenValidationParameters.ActorValidationParameters will not find signing key, but Actor should not be validated
                claimsIdentity = new ClaimsIdentity(ClaimSets.DefaultClaimsIdentity);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Actor, Default.SymmetricJws));
                validationParameters = Default.AsymmetricSignTokenValidationParameters;
                validationParameters.ValidateActor = false;
                validationParameters.ActorValidationParameters = Default.AsymmetricSignTokenValidationParameters;
                theoryData.Add(
                    new JwtTheoryData
                    {                     
                        TestId = "ActorValidationFalse",
                        ExpectedException = ExpectedException.NoExceptionExpected,
                        Token = handler.CreateEncodedJwt(Default.Issuer, Default.Audience, claimsIdentity, null, null, null, Default.AsymmetricSigningCredentials),
                        TokenHandler = handler,
                        ValidationParameters = validationParameters
                    }
                );

                // Actor validation is true
                // Actor will be validated using validationsParameters.ActorValidationParameters
                claimsIdentity = new ClaimsIdentity(ClaimSets.DefaultClaimsIdentity);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Actor, Default.SymmetricJws));
                validationParameters = Default.AsymmetricSignTokenValidationParameters;
                validationParameters.ActorValidationParameters = Default.SymmetricSignTokenValidationParameters;
                validationParameters.ValidateActor = true;
                theoryData.Add(
                    new JwtTheoryData
                    {
                        TestId = "ActorValidationUsingActorTVP - True",
                        ExpectedException = ExpectedException.NoExceptionExpected,
                        Token = handler.CreateEncodedJwt(Default.Issuer, Default.Audience, claimsIdentity, null, null, null, Default.AsymmetricSigningCredentials),
                        TokenHandler = handler,
                        ValidationParameters = validationParameters
                    }
                );

                return theoryData;
            }
        }

        [Theory, MemberData(nameof(BootstrapContextTheoryData))]
        public void BootstrapContext(JwtTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.BootstrapContext", theoryData);

            var claimsPrincipal = theoryData.TokenHandler.ValidateToken(theoryData.Token, theoryData.ValidationParameters, out SecurityToken securityToken);
            var bootstrapContext = (claimsPrincipal.Identity as ClaimsIdentity).BootstrapContext as string;
            if (theoryData.ValidationParameters.SaveSigninToken)
            {
                Assert.NotNull(bootstrapContext);
                Assert.True(IdentityComparer.AreEqual(claimsPrincipal, theoryData.TokenHandler.ValidateToken(bootstrapContext, theoryData.ValidationParameters, out SecurityToken validatedToken)));
            }
            else
            {
                Assert.Null(bootstrapContext);
            }
        }

        public static TheoryData<JwtTheoryData> BootstrapContextTheoryData
        {
            get
            {
                var theoryData = new TheoryData<JwtTheoryData>();
                var validationParameters = Default.AsymmetricEncryptSignTokenValidationParameters;
                validationParameters.SaveSigninToken = true;
                theoryData.Add(new JwtTheoryData
                {
                    Token = Default.AsymmetricJwt,
                    ValidationParameters = Default.AsymmetricEncryptSignTokenValidationParameters,
                });

                validationParameters = Default.AsymmetricEncryptSignTokenValidationParameters;
                validationParameters.SaveSigninToken = false;
                theoryData.Add(new JwtTheoryData
                {
                    Token = Default.AsymmetricJwt,
                    ValidationParameters = validationParameters
                });

                return theoryData;
            }
        }

        [Fact]
        public void OutboundHeaderMappingInstanceTesting()
        {
            var handler1 = new JwtSecurityTokenHandler();
            var handler2 = new JwtSecurityTokenHandler();

            handler1.OutboundAlgorithmMap[SecurityAlgorithms.Aes128Encryption] = SecurityAlgorithms.EcdsaSha256;
            Assert.True(handler1.OutboundAlgorithmMap.ContainsKey(SecurityAlgorithms.Aes128Encryption));
            Assert.False(handler2.OutboundAlgorithmMap.ContainsKey(SecurityAlgorithms.Aes128Encryption));

            var header = new JwtHeader(
                new SigningCredentials(KeyingMaterial.Ecdsa256Key, SecurityAlgorithms.Aes128Encryption),
                handler1.OutboundAlgorithmMap);

            Assert.True(header.Alg == SecurityAlgorithms.EcdsaSha256);

            header = new JwtHeader(
                new SigningCredentials(KeyingMaterial.Ecdsa256Key, SecurityAlgorithms.Aes128Encryption),
                handler2.OutboundAlgorithmMap);

            Assert.True(header.Alg == SecurityAlgorithms.Aes128Encryption);
        }

        [Theory]
        [InlineData(SecurityAlgorithms.EcdsaSha256Signature, SecurityAlgorithms.EcdsaSha256)]
        [InlineData(SecurityAlgorithms.EcdsaSha384Signature, SecurityAlgorithms.EcdsaSha384)]
        [InlineData(SecurityAlgorithms.EcdsaSha512Signature, SecurityAlgorithms.EcdsaSha512)]
        [InlineData(SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.HmacSha256)]
        [InlineData(SecurityAlgorithms.HmacSha384Signature, SecurityAlgorithms.HmacSha384)]
        [InlineData(SecurityAlgorithms.HmacSha512Signature, SecurityAlgorithms.HmacSha512)]
        [InlineData(SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.RsaSha256)]
        [InlineData(SecurityAlgorithms.RsaSha384Signature, SecurityAlgorithms.RsaSha384)]
        [InlineData(SecurityAlgorithms.RsaSha512Signature, SecurityAlgorithms.RsaSha512)]
        [InlineData(SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha256)]
        [InlineData(SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha384)]
        [InlineData(SecurityAlgorithms.EcdsaSha512, SecurityAlgorithms.EcdsaSha512)]
        [InlineData(SecurityAlgorithms.HmacSha256, SecurityAlgorithms.HmacSha256)]
        [InlineData(SecurityAlgorithms.HmacSha384, SecurityAlgorithms.HmacSha384)]
        [InlineData(SecurityAlgorithms.HmacSha512, SecurityAlgorithms.HmacSha512)]
        [InlineData(SecurityAlgorithms.RsaSha256, SecurityAlgorithms.RsaSha256)]
        [InlineData(SecurityAlgorithms.RsaSha384, SecurityAlgorithms.RsaSha384)]
        [InlineData(SecurityAlgorithms.RsaSha512, SecurityAlgorithms.RsaSha512)]
        [InlineData(SecurityAlgorithms.Aes128Encryption, SecurityAlgorithms.Aes128Encryption)]
        public void OutboundHeaderMappingCreateHeader(string outboundAlgorithm, string expectedValue)
        {
            var handler = new JwtSecurityTokenHandler();
            var header = new JwtHeader(
                            new SigningCredentials(KeyingMaterial.Ecdsa256Key, outboundAlgorithm),
                            handler.OutboundAlgorithmMap);

            Assert.True(header.Alg == expectedValue);
        }

        [Theory]
        [InlineData(SecurityAlgorithms.EcdsaSha256Signature, SecurityAlgorithms.EcdsaSha256)]
        [InlineData(SecurityAlgorithms.EcdsaSha384Signature, SecurityAlgorithms.EcdsaSha384)]
        [InlineData(SecurityAlgorithms.EcdsaSha512Signature, SecurityAlgorithms.EcdsaSha512)]
        [InlineData(SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.HmacSha256)]
        [InlineData(SecurityAlgorithms.HmacSha384Signature, SecurityAlgorithms.HmacSha384)]
        [InlineData(SecurityAlgorithms.HmacSha512Signature, SecurityAlgorithms.HmacSha512)]
        [InlineData(SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.RsaSha256)]
        [InlineData(SecurityAlgorithms.RsaSha384Signature, SecurityAlgorithms.RsaSha384)]
        [InlineData(SecurityAlgorithms.RsaSha512Signature, SecurityAlgorithms.RsaSha512)]
        public void OutboundHeaderMappingCreateToken(string outboundAlgorithm, string expectedValue)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = null;

            switch (outboundAlgorithm)
            {
                case SecurityAlgorithms.EcdsaSha256Signature:
                    jwt = handler.CreateJwtSecurityToken(new SecurityTokenDescriptor { SigningCredentials = new SigningCredentials(KeyingMaterial.Ecdsa256Key, outboundAlgorithm) });
                    break;
                case SecurityAlgorithms.EcdsaSha384Signature:
                    jwt = handler.CreateJwtSecurityToken(new SecurityTokenDescriptor { SigningCredentials = new SigningCredentials(KeyingMaterial.Ecdsa384Key, outboundAlgorithm) });
                    break;
                case SecurityAlgorithms.EcdsaSha512Signature:
                    jwt = handler.CreateJwtSecurityToken(new SecurityTokenDescriptor { SigningCredentials = new SigningCredentials(KeyingMaterial.Ecdsa521Key, outboundAlgorithm) });
                    break;

                case SecurityAlgorithms.RsaSha256Signature:
                case SecurityAlgorithms.RsaSha384Signature:
                case SecurityAlgorithms.RsaSha512Signature:
                    jwt = handler.CreateJwtSecurityToken(new SecurityTokenDescriptor { SigningCredentials = new SigningCredentials(KeyingMaterial.RsaSecurityKey_2048, outboundAlgorithm) });
                    break;

                case SecurityAlgorithms.HmacSha256Signature:
                case SecurityAlgorithms.HmacSha384Signature:
                case SecurityAlgorithms.HmacSha512Signature:
                    jwt = handler.CreateJwtSecurityToken(new SecurityTokenDescriptor { SigningCredentials = new SigningCredentials(KeyingMaterial.SymmetricSecurityKey2_256, outboundAlgorithm) });
                    break;
            }

            Assert.True(jwt.Header.Alg == expectedValue);
        }

        [Fact]
        public void InboundOutboundClaimTypeMapping()
        {
            List<KeyValuePair<string, string>> aadStrings = new List<KeyValuePair<string, string>>();
            aadStrings.Add(new KeyValuePair<string, string>("amr", "http://schemas.microsoft.com/claims/authnmethodsreferences"));
            aadStrings.Add(new KeyValuePair<string, string>("deviceid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/identifier"));
            aadStrings.Add(new KeyValuePair<string, string>("family_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"));
            aadStrings.Add(new KeyValuePair<string, string>("given_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"));
            aadStrings.Add(new KeyValuePair<string, string>("idp", "http://schemas.microsoft.com/identity/claims/identityprovider"));
            aadStrings.Add(new KeyValuePair<string, string>("oid", "http://schemas.microsoft.com/identity/claims/objectidentifier"));
            aadStrings.Add(new KeyValuePair<string, string>("scp", "http://schemas.microsoft.com/identity/claims/scope"));
            aadStrings.Add(new KeyValuePair<string, string>("tid", "http://schemas.microsoft.com/identity/claims/tenantid"));
            aadStrings.Add(new KeyValuePair<string, string>("unique_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"));
            aadStrings.Add(new KeyValuePair<string, string>("upn", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"));

            foreach (var kv in aadStrings)
            {
                Assert.True(JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.ContainsKey(kv.Key), "Inbound short type missing: " + kv.Key);
                Assert.True(JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[kv.Key] == kv.Value, "Inbound mapping wrong: key " + kv.Key + " expected: " + JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[kv.Key] + ", received: " + kv.Value);
            }

            List<KeyValuePair<string, string>> adfsStrings = new List<KeyValuePair<string, string>>();
            adfsStrings.Add(new KeyValuePair<string, string>("pwdexptime", "http://schemas.microsoft.com/ws/2012/01/passwordexpirationtime"));
            adfsStrings.Add(new KeyValuePair<string, string>("pwdexpdays", "http://schemas.microsoft.com/ws/2012/01/passwordexpirationdays"));
            adfsStrings.Add(new KeyValuePair<string, string>("pwdchgurl", "http://schemas.microsoft.com/ws/2012/01/passwordchangeurl"));
            adfsStrings.Add(new KeyValuePair<string, string>("clientip", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-ip"));
            adfsStrings.Add(new KeyValuePair<string, string>("forwardedclientip", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-forwarded-client-ip"));
            adfsStrings.Add(new KeyValuePair<string, string>("clientapplication", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-application"));
            adfsStrings.Add(new KeyValuePair<string, string>("clientuseragent", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-user-agent"));
            adfsStrings.Add(new KeyValuePair<string, string>("endpointpath", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-endpoint-absolute-path"));
            adfsStrings.Add(new KeyValuePair<string, string>("proxy", "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-proxy"));
            adfsStrings.Add(new KeyValuePair<string, string>("relyingpartytrustid", "http://schemas.microsoft.com/2012/01/requestcontext/claims/relyingpartytrustid"));
            adfsStrings.Add(new KeyValuePair<string, string>("insidecorporatenetwork", "http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork"));
            adfsStrings.Add(new KeyValuePair<string, string>("isregistereduser", "http://schemas.microsoft.com/2012/01/devicecontext/claims/isregistereduser"));
            adfsStrings.Add(new KeyValuePair<string, string>("deviceowner", "http://schemas.microsoft.com/2012/01/devicecontext/claims/userowner"));
            adfsStrings.Add(new KeyValuePair<string, string>("deviceid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/identifier"));
            adfsStrings.Add(new KeyValuePair<string, string>("deviceregid", "http://schemas.microsoft.com/2012/01/devicecontext/claims/registrationid"));
            adfsStrings.Add(new KeyValuePair<string, string>("devicedispname", "http://schemas.microsoft.com/2012/01/devicecontext/claims/displayname"));
            adfsStrings.Add(new KeyValuePair<string, string>("deviceosver", "http://schemas.microsoft.com/2012/01/devicecontext/claims/osversion"));
            adfsStrings.Add(new KeyValuePair<string, string>("deviceismanaged", "http://schemas.microsoft.com/2012/01/devicecontext/claims/ismanaged"));
            adfsStrings.Add(new KeyValuePair<string, string>("deviceostype", "http://schemas.microsoft.com/2012/01/devicecontext/claims/ostype"));
            adfsStrings.Add(new KeyValuePair<string, string>("authmethod", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod"));
            adfsStrings.Add(new KeyValuePair<string, string>("email", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"));
            adfsStrings.Add(new KeyValuePair<string, string>("given_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"));
            adfsStrings.Add(new KeyValuePair<string, string>("unique_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"));
            adfsStrings.Add(new KeyValuePair<string, string>("upn", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"));
            adfsStrings.Add(new KeyValuePair<string, string>("commonname", "http://schemas.xmlsoap.org/claims/CommonName"));
            adfsStrings.Add(new KeyValuePair<string, string>("adfs1email", "http://schemas.xmlsoap.org/claims/EmailAddress"));
            adfsStrings.Add(new KeyValuePair<string, string>("group", "http://schemas.xmlsoap.org/claims/Group"));
            adfsStrings.Add(new KeyValuePair<string, string>("adfs1upn", "http://schemas.xmlsoap.org/claims/UPN"));
            adfsStrings.Add(new KeyValuePair<string, string>("role", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"));
            adfsStrings.Add(new KeyValuePair<string, string>("family_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"));
            adfsStrings.Add(new KeyValuePair<string, string>("ppid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier"));
            adfsStrings.Add(new KeyValuePair<string, string>("nameid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"));
            adfsStrings.Add(new KeyValuePair<string, string>("denyonlysid", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid"));
            adfsStrings.Add(new KeyValuePair<string, string>("denyonlyprimarysid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarysid"));
            adfsStrings.Add(new KeyValuePair<string, string>("denyonlyprimarygroupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarygroupsid"));
            adfsStrings.Add(new KeyValuePair<string, string>("groupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid"));
            adfsStrings.Add(new KeyValuePair<string, string>("primarygroupsid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid"));
            adfsStrings.Add(new KeyValuePair<string, string>("primarysid", "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid"));
            adfsStrings.Add(new KeyValuePair<string, string>("winaccountname", "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"));
            adfsStrings.Add(new KeyValuePair<string, string>("certapppolicy", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/applicationpolicy"));
            adfsStrings.Add(new KeyValuePair<string, string>("certauthoritykeyidentifier", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/authoritykeyidentifier"));
            adfsStrings.Add(new KeyValuePair<string, string>("certbasicconstraints", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/basicconstraints"));
            adfsStrings.Add(new KeyValuePair<string, string>("certeku", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/eku"));
            adfsStrings.Add(new KeyValuePair<string, string>("certissuer", "http://schemas.microsoft.com/2012/12/certificatecontext/field/issuer"));
            adfsStrings.Add(new KeyValuePair<string, string>("certissuername", "http://schemas.microsoft.com/2012/12/certificatecontext/field/issuername"));
            adfsStrings.Add(new KeyValuePair<string, string>("certkeyusage", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/keyusage"));
            adfsStrings.Add(new KeyValuePair<string, string>("certnotafter", "http://schemas.microsoft.com/2012/12/certificatecontext/field/notafter"));
            adfsStrings.Add(new KeyValuePair<string, string>("certnotbefore", "http://schemas.microsoft.com/2012/12/certificatecontext/field/notbefore"));
            adfsStrings.Add(new KeyValuePair<string, string>("certpolicy", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatepolicy"));
            adfsStrings.Add(new KeyValuePair<string, string>("certpublickey", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa"));
            adfsStrings.Add(new KeyValuePair<string, string>("certrawdata", "http://schemas.microsoft.com/2012/12/certificatecontext/field/rawdata"));
            adfsStrings.Add(new KeyValuePair<string, string>("certsubjectaltname", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/san"));
            adfsStrings.Add(new KeyValuePair<string, string>("certserialnumber", "http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber"));
            adfsStrings.Add(new KeyValuePair<string, string>("certsignaturealgorithm", "http://schemas.microsoft.com/2012/12/certificatecontext/field/signaturealgorithm"));
            adfsStrings.Add(new KeyValuePair<string, string>("certsubject", "http://schemas.microsoft.com/2012/12/certificatecontext/field/subject"));
            adfsStrings.Add(new KeyValuePair<string, string>("certsubjectkeyidentifier", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/subjectkeyidentifier"));
            adfsStrings.Add(new KeyValuePair<string, string>("certsubjectname", "http://schemas.microsoft.com/2012/12/certificatecontext/field/subjectname"));
            adfsStrings.Add(new KeyValuePair<string, string>("certtemplateinformation", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatetemplateinformation"));
            adfsStrings.Add(new KeyValuePair<string, string>("certtemplatename", "http://schemas.microsoft.com/2012/12/certificatecontext/extension/certificatetemplatename"));
            adfsStrings.Add(new KeyValuePair<string, string>("certthumbprint", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint"));
            adfsStrings.Add(new KeyValuePair<string, string>("certx509version", "http://schemas.microsoft.com/2012/12/certificatecontext/field/x509version"));
            adfsStrings.Add(new KeyValuePair<string, string>("acr", "http://schemas.microsoft.com/claims/authnclassreference"));
            adfsStrings.Add(new KeyValuePair<string, string>("amr", "http://schemas.microsoft.com/claims/authnmethodsreferences"));

            foreach (var kv in adfsStrings)
            {
                Assert.True(JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.ContainsKey(kv.Key), "Inbound short type missing: '" + kv.Key + "'");
                Assert.True(JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[kv.Key] == kv.Value, "Inbound mapping wrong: key '" + kv.Key + "' expected: " + JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[kv.Key] + ", received: '" + kv.Value + "'");
            }

            var handler = new JwtSecurityTokenHandler();

            List<Claim> expectedInboundClaimsMapped = new List<Claim>(
                ClaimSets.ExpectedInClaimsIdentityUsingAllInboundShortClaimTypes(
                        Default.Issuer,
                        Default.Issuer
                        ));

            var jwt = handler.CreateJwtSecurityToken(
                issuer: Default.Issuer,
                audience: Default.Audience,
                subject: new ClaimsIdentity(
                    ClaimSets.AllInboundShortClaimTypes(
                        Default.Issuer,
                        Default.Issuer)));

            List<Claim> expectedInboundClaimsUnMapped = new List<Claim>(
                    ClaimSets.AllInboundShortClaimTypes(
                        Default.Issuer,
                        Default.Issuer
                        ));

            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = false,
                RequireSignedTokens = false,
                ValidateAudience = false,
                ValidateIssuer = false,
            };

            handler.InboundClaimFilter.Add("aud");
            handler.InboundClaimFilter.Add("exp");
            handler.InboundClaimFilter.Add("iat");
            handler.InboundClaimFilter.Add("iss");
            handler.InboundClaimFilter.Add("nbf");

            // ValidateToken will map claims according to the InboundClaimTypeMap
            RunClaimMappingVariation(jwt: jwt, tokenHandler: handler, validationParameters: validationParameters, expectedClaims: expectedInboundClaimsMapped, identityName: ClaimTypes.Name);

            handler.InboundClaimTypeMap.Clear();
            RunClaimMappingVariation(jwt, handler, validationParameters, expectedClaims: expectedInboundClaimsUnMapped, identityName: null);

            // test that setting the NameClaimType override works.
            List<Claim> claims = new List<Claim>()
            {
                new Claim( JwtRegisteredClaimNames.Email, "Bob", ClaimValueTypes.String, Default.Issuer, Default.Issuer ),
                new Claim( ClaimTypes.Spn, "spn", ClaimValueTypes.String, Default.Issuer, Default.Issuer ),
                new Claim( JwtRegisteredClaimNames.Sub, "Subject1", ClaimValueTypes.String, Default.Issuer, Default.Issuer ),
                new Claim( JwtRegisteredClaimNames.Prn, "Principal1", ClaimValueTypes.String, Default.Issuer, Default.Issuer ),
                new Claim( JwtRegisteredClaimNames.Sub, "Subject2", ClaimValueTypes.String, Default.Issuer, Default.Issuer ),
            };


            handler = new JwtSecurityTokenHandler();
            handler.InboundClaimFilter.Add("exp");
            handler.InboundClaimFilter.Add("nbf");
            handler.InboundClaimFilter.Add("iat");
            handler.InboundClaimTypeMap = new Dictionary<string, string>()
            {
                { JwtRegisteredClaimNames.Email, "Mapped_" + JwtRegisteredClaimNames.Email },
                { JwtRegisteredClaimNames.GivenName, "Mapped_" + JwtRegisteredClaimNames.GivenName },
                { JwtRegisteredClaimNames.Prn, "Mapped_" + JwtRegisteredClaimNames.Prn },
                { JwtRegisteredClaimNames.Sub, "Mapped_" + JwtRegisteredClaimNames.Sub },
            };

            jwt = handler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience, subject: new ClaimsIdentity(claims));

            List<Claim> expectedClaims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Iss, Default.Issuer, ClaimValueTypes.String, Default.Issuer, Default.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, Default.Audience, ClaimValueTypes.String, Default.Issuer, Default.Issuer),
                new Claim(ClaimTypes.Spn, "spn", ClaimValueTypes.String, Default.Issuer, Default.Issuer),
            };

            Claim claim = null;
            claim = new Claim("Mapped_" + JwtRegisteredClaimNames.Email, "Bob", ClaimValueTypes.String, Default.Issuer, Default.Issuer);
            claim.Properties.Add(new KeyValuePair<string, string>(JwtSecurityTokenHandler.ShortClaimTypeProperty, JwtRegisteredClaimNames.Email));
            expectedClaims.Add(claim);

            claim = new Claim("Mapped_" + JwtRegisteredClaimNames.Sub, "Subject1", ClaimValueTypes.String, Default.Issuer, Default.Issuer);
            claim.Properties.Add(new KeyValuePair<string, string>(JwtSecurityTokenHandler.ShortClaimTypeProperty, JwtRegisteredClaimNames.Sub));
            expectedClaims.Add(claim);

            claim = new Claim("Mapped_" + JwtRegisteredClaimNames.Prn, "Principal1", ClaimValueTypes.String, Default.Issuer, Default.Issuer);
            claim.Properties.Add(new KeyValuePair<string, string>(JwtSecurityTokenHandler.ShortClaimTypeProperty, JwtRegisteredClaimNames.Prn));
            expectedClaims.Add(claim);

            claim = new Claim("Mapped_" + JwtRegisteredClaimNames.Sub, "Subject2", ClaimValueTypes.String, Default.Issuer, Default.Issuer);
            claim.Properties.Add(new KeyValuePair<string, string>(JwtSecurityTokenHandler.ShortClaimTypeProperty, JwtRegisteredClaimNames.Sub));
            expectedClaims.Add(claim);

            RunClaimMappingVariation(jwt, handler, validationParameters, expectedClaims: expectedClaims, identityName: null);
        }

        [Fact]
        public void MapInboundClaims()
        {
            var handler = new JwtSecurityTokenHandler();

            // By default, JwtSecurityTokenHandler.DefaultMapInboundClaims should be true so make sure we initialize the InboundClaimTypeMap with the default mappings.
            Assert.Equal(73, handler.InboundClaimTypeMap.Count);

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            handler = new JwtSecurityTokenHandler();

            // Make sure that we don't populate the InboundClaimTypeMap if DefaultMapInboundClaims was previously set to false.
            Assert.Equal(0, handler.InboundClaimTypeMap.Count);

            var claims = new List<Claim>
            {
             new Claim(JwtRegisteredClaimNames.Email, "Bob@contoso.com", ClaimValueTypes.String, Default.Issuer),
             new Claim(JwtRegisteredClaimNames.GivenName, "Bob", ClaimValueTypes.String, Default.Issuer)
            };

            var jwt = handler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience, subject: new ClaimsIdentity(claims));
         
            // Check to make sure none of the short claim types have been mapped to longer ones.
            foreach (var claim in claims)
                jwt.Claims.Single(s => s.Type == claim.Type);

            handler.MapInboundClaims = true;

            // Check to make sure that setting MapInboundClaims to true initializes the InboundClaimType map with the default mappings if it was previously empty.
            Assert.Equal(73, handler.InboundClaimTypeMap.Count);
            // Check to make sure that changing the instance property did not alter the static property.
            Assert.True(JwtSecurityTokenHandler.DefaultMapInboundClaims == false);
        }

        [Theory, MemberData(nameof(ReadTimesExpressedAsDoublesTheoryData))]
        public void ReadTimesExpressedAsDoubles(JwtTheoryData theoryData)
        {
            var context = TestUtilities.WriteHeader($"{this}.ReadTimesExpressedAsDoubles", theoryData);
            try
            {
                var principal = theoryData.TokenHandler.ValidateToken(theoryData.Token, theoryData.ValidationParameters, out SecurityToken validToken);

                var jwtToken1 = validToken as JwtSecurityToken;
                var jwtToken2 = theoryData.SecurityToken as JwtSecurityToken;

                IdentityComparer.AreEqual(jwtToken1.Payload[JwtRegisteredClaimNames.Nbf], jwtToken2.Payload[JwtRegisteredClaimNames.Nbf], context);
                IdentityComparer.AreEqual(jwtToken1.Payload[JwtRegisteredClaimNames.Exp], jwtToken2.Payload[JwtRegisteredClaimNames.Exp], context);
                IdentityComparer.AreEqual(jwtToken1.Payload[JwtRegisteredClaimNames.Iat], jwtToken2.Payload[JwtRegisteredClaimNames.Iat], context);

                theoryData.ExpectedException.ProcessNoException(context);
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex, context);
            }

            TestUtilities.AssertFailIfErrors(context);
        }

        public static TheoryData<JwtTheoryData> ReadTimesExpressedAsDoublesTheoryData
        {
            get
            {
                TimeSpan timeSpan = DateTime.UtcNow.ToUniversalTime().AddMinutes(10) - EpochTime.UnixEpoch;
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

                JwtSecurityToken jwtToken1 = handler.CreateToken(new SecurityTokenDescriptor()) as JwtSecurityToken;
                jwtToken1.Payload[JwtRegisteredClaimNames.Nbf] = 1471924298.67746;
                jwtToken1.Payload[JwtRegisteredClaimNames.Iat] = jwtToken1.Payload[JwtRegisteredClaimNames.Nbf];
                jwtToken1.Payload[JwtRegisteredClaimNames.Exp] = timeSpan.TotalSeconds;

                var payload = new JwtPayload();
                payload[JwtRegisteredClaimNames.Nbf] = "1472096544.75759";
                payload[JwtRegisteredClaimNames.Iat] = "1472096557.74376";
                payload[JwtRegisteredClaimNames.Exp] = "1472097744.75859";
                JwtSecurityToken jwtToken2 = new JwtSecurityToken(new JwtHeader(), payload);

                var token2 = @"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzUxMiIsImlzRW5jcnlwdGVkIjoiRmFsc2UiLCJ4NXQiOiI1M0VENjE1NTUwNTlBRDg3QUE4MkNBNTYwRTQ4QkIxMkM1MzdGOUY1IiwidmVyIjoiMi4xIn0.eyJhdWQiOiJIZWxpeC5TZWN1cml0eS5VdGlsIiwiaWF0IjoiMTQ3MjA5NjU1Ny43NDM3NiIsIm5iZiI6IjE0NzIwOTY1NDQuNzU3NTkiLCJDbGFpbVNldHMiOlt7IkNsYWltcyI6eyJzZXJ2ZXJJZCI6IkhlbGl4LkNvbnRhaW5lcnMuRGV2Iiwic2VydmVyVmVyc2lvbiI6ImRldiIsImlzc1g1dCI6IjUzRUQ2MTU1NTA1OUFEODdBQTgyQ0E1NjBFNDhCQjEyQzUzN0Y5RjUifSwiUHJvdmlkZXIiOiJIZWxpeC5Db250YWluZXJzLkRldi52ZGV2IiwiU2NoZW1hIjoiSGVsaXguQ29udGFpbmVyIiwiVmVyc2lvbiI6IlYxIn1dLCJpc3MiOiJIZWxpeC5Db250YWluZXJzLkRldi52ZGV2IiwiZXhwIjoiMTQ3MjA5Nzc0NC43NTg1OSIsInNzaWQiOiJjNmJkNzY3ZjE4YWU0ZTQyOTliMmY4YjJmNzhmODU1NSJ9.W8ARsO3IKMO_CBl5fMkgTEkPmoZZvjaX46-mmVHqT5hQAbQVBmnc18B9VxsSS34YKVE2dBQwZHjhu2ROSOCKeuHOqHjjS_HuSdDLOdi7rJUdpKw1GE-lBqxzUPojAlUvLRlq7KjbwipXd7bJyMk7chVU9r548pmljDAlm7SOqmM-qcZ8X0sgQcDxxZoacJiL9xQpbJPi9CVHC_ms2LJhm6AFcCNTlRZNgAmMvoIBWfjXVsVC92HFgqd_qTpMvudTs216LIfslpJC0WiU4SFWKV2Bt5rGGVVqSe4vXb4W1Si58t8ORcepRnkZ1jkEuKf2VpHTEw0ylwX_BLqnnKdavQ";
                int index = token2.LastIndexOf('.');
                token2 = token2.Substring(0, index + 1);

                return new TheoryData<JwtTheoryData>
                {
                    new JwtTheoryData
                    {
                        First = true,
                        SecurityToken = jwtToken1,
                        TestId = "CreateTokenFromTokenHandler",
                        Token = handler.WriteToken(jwtToken1),
                        ValidationParameters = new TokenValidationParameters
                        {
                            RequireExpirationTime = false,
                            RequireSignedTokens = false,
                            ValidateAudience = false,
                            ValidateIssuer = false,
                        }
                    },

                    new JwtTheoryData
                    {
                        SecurityToken = jwtToken2,
                        TestId = "TokenFromCustomerReport",
                        Token = token2,
                        ValidationParameters = new TokenValidationParameters
                        {
                            RequireExpirationTime = false,
                            RequireSignedTokens = false,
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ClockSkew = timeSpan
                        }
                    }
                };
            }
        }

        private void RunClaimMappingVariation(JwtSecurityToken jwt, JwtSecurityTokenHandler tokenHandler, TokenValidationParameters validationParameters, IEnumerable<Claim> expectedClaims, string identityName)
        {
            SecurityToken validatedToken;
            ClaimsPrincipal cp = tokenHandler.ValidateToken(jwt.RawData, validationParameters, out validatedToken);
            ClaimsIdentity identity = cp.Identity as ClaimsIdentity;

            Assert.True(IdentityComparer.AreEqual(identity.Claims, expectedClaims, new CompareContext { IgnoreType = true }), "identity.Claims != expectedClaims");
            Assert.Equal(identity.Name, identityName);

            // This checks that all claims that should have been mapped.
            foreach (Claim claim in identity.Claims)
            {
                // if it was mapped, make sure the shortname is found in the mapping and equals the claim.Type
                if (claim.Properties.ContainsKey(JwtSecurityTokenHandler.ShortClaimTypeProperty))
                {
                    Assert.True(tokenHandler.InboundClaimTypeMap.ContainsKey(claim.Properties[JwtSecurityTokenHandler.ShortClaimTypeProperty]), "!JwtSecurityTokenHandler.InboundClaimTypeMap.ContainsKey( claim.Properties[JwtSecurityTokenHandler.ShortClaimTypeProperty] ): " + claim.Type);
                }
                // there was no short property.
                Assert.False(tokenHandler.InboundClaimTypeMap.ContainsKey(claim.Type), "JwtSecurityTokenHandler.InboundClaimTypeMap.ContainsKey( claim.Type ), wasn't mapped claim.Type: " + claim.Type);
            }

            foreach (Claim claim in jwt.Claims)
            {
                string claimType = claim.Type;

                if (tokenHandler.InboundClaimTypeMap.ContainsKey(claimType))
                {
                    claimType = tokenHandler.InboundClaimTypeMap[claim.Type];
                }

                if (!tokenHandler.InboundClaimFilter.Contains(claim.Type))
                {
                    Claim firstClaim = identity.FindFirst(claimType);
                    Assert.True(firstClaim != null, "Claim firstClaim = identity.FindFirst( claimType ), firstClaim == null. claim.Type: " + claim.Type + " claimType: " + claimType);
                }
            }
        }

        [Fact]
        public void InstanceClaimMappingAndFiltering()
        {
            // testing if one handler overrides instance claim type map of another
            JwtSecurityTokenHandler handler1 = new JwtSecurityTokenHandler();
            JwtSecurityTokenHandler handler2 = new JwtSecurityTokenHandler();
            Assert.True(handler1.InboundClaimTypeMap.Count != 0, "handler1 should not have an empty inbound claim type map");
            handler1.InboundClaimTypeMap.Clear();
            Assert.True(handler1.InboundClaimTypeMap.Count == 0, "handler1 should have an empty inbound claim type map");
            Assert.True(handler2.InboundClaimTypeMap.Count != 0, "handler2 should not have an empty inbound claim type map");

            // Setup
            var jwtClaim = new Claim("jwtClaim", "claimValue");
            var internalClaim = new Claim("internalClaim", "claimValue");
            var unwantedClaim = new Claim("unwantedClaim", "unwantedValue");
            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimFilter = new HashSet<string>();
            handler.InboundClaimTypeMap = new Dictionary<string, string>();
            handler.OutboundClaimTypeMap = new Dictionary<string, string>();

            handler.InboundClaimFilter.Add("unwantedClaim");
            handler.InboundClaimTypeMap.Add("jwtClaim", "internalClaim");
            handler.OutboundClaimTypeMap.Add("internalClaim", "jwtClaim");

            // Test outgoing
            var outgoingToken = handler.CreateJwtSecurityToken(subject: new ClaimsIdentity(new Claim[] { internalClaim }));
            var wasClaimMapped = System.Linq.Enumerable.Contains<Claim>(outgoingToken.Claims, jwtClaim, new ClaimComparer());
            Assert.True(wasClaimMapped);

            // Test incoming
            var incomingToken = handler.CreateJwtSecurityToken(issuer: "Test Issuer", subject: new ClaimsIdentity(new Claim[] { jwtClaim, unwantedClaim }));
            var validationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = false,
                ValidateAudience = false,
                ValidateIssuer = false
            };
            SecurityToken token;
            var identity = handler.ValidateToken(incomingToken.RawData, validationParameters, out token);
            Assert.False(identity.HasClaim(c => c.Type == "unwantedClaim"));
            Assert.False(identity.HasClaim(c => c.Type == "jwtClaim"));
            Assert.True(identity.HasClaim("internalClaim", "claimValue"));
        }

        [Theory, MemberData(nameof(JWEDecompressionTheoryData))]
        public void JWEDecompressionTest(JWEDecompressionTheoryData theoryData)
        {
            var context = TestUtilities.WriteHeader($"{this}.JWEDecompressionTest", theoryData);

            try
            {
                var handler = new JwtSecurityTokenHandler();
                CompressionProviderFactory.Default = theoryData.CompressionProviderFactory;
                var claimsPrincipal = handler.ValidateToken(theoryData.JWECompressionString, theoryData.ValidationParameters, out var validatedToken);

                if (!claimsPrincipal.Claims.Any())
                        context.Diffs.Add("claimsPrincipal.Claims is empty.");

                theoryData.ExpectedException.ProcessNoException(context);
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex, context);
            }

            TestUtilities.AssertFailIfErrors(context);
        }

        public static TheoryData<JWEDecompressionTheoryData> JWEDecompressionTheoryData()
        {
            var compressionProviderFactoryForCustom = new CompressionProviderFactory()
            {
                CustomCompressionProvider = new SampleCustomCompressionProvider("MyAlgorithm")
            };

            var compressionProviderFactoryForCustom2 = new CompressionProviderFactory()
            {
                CustomCompressionProvider = new SampleCustomCompressionProviderDecompressAndCompressAlwaysFail("MyAlgorithm")
            };

            return new TheoryData<JWEDecompressionTheoryData>() {
                new JWEDecompressionTheoryData
                {
                    ValidationParameters = Default.JWECompressionTokenValidationParameters,
                    JWECompressionString = ReferenceTokens.JWECompressionTokenWithDEF,
                    CompressionProviderFactory = CompressionProviderFactory.Default,
                    TestId = "ValidAlgorithm"
                },
                new JWEDecompressionTheoryData
                {
                    ValidationParameters = Default.JWECompressionTokenValidationParameters,
                    JWECompressionString = ReferenceTokens.JWECompressionTokenWithUnsupportedAlgorithm,
                    CompressionProviderFactory = CompressionProviderFactory.Default,
                    TestId = "InvalidAlgorithm",
                    ExpectedException = new ExpectedException(typeof(SecurityTokenDecompressionFailedException), "IDX10679:", typeof(NotSupportedException))
                },
                new JWEDecompressionTheoryData
                {
                    ValidationParameters = Default.JWECompressionTokenValidationParameters,
                    JWECompressionString = ReferenceTokens.JWEInvalidCompressionTokenWithDEF,
                    CompressionProviderFactory = CompressionProviderFactory.Default,
                    TestId = "InvalidToken",
                    ExpectedException = new ExpectedException(typeof(SecurityTokenDecompressionFailedException), "IDX10679:", typeof(InvalidDataException))
                },
                new JWEDecompressionTheoryData
                {
                    ValidationParameters = Default.JWECompressionTokenValidationParameters,
                    JWECompressionString = ReferenceTokens.JWECompressionTokenWithDEF,
                    CompressionProviderFactory = null,
                    TestId = "NullCompressionProviderFactory",
                    ExpectedException = ExpectedException.ArgumentNullException("IDX10000:")
                },
                new JWEDecompressionTheoryData
                {
                    ValidationParameters = Default.JWECompressionTokenValidationParameters,
                    CompressionProviderFactory = compressionProviderFactoryForCustom,
                    JWECompressionString = ReferenceTokens.JWECompressionTokenWithCustomAlgorithm,
                    TestId = "CustomCompressionProviderSucceeds"
                },
                new JWEDecompressionTheoryData
                {
                    ValidationParameters = Default.JWECompressionTokenValidationParameters,
                    JWECompressionString = ReferenceTokens.JWECompressionTokenWithDEF,
                    CompressionProviderFactory = compressionProviderFactoryForCustom2,
                    TestId = "CustomCompressionProviderFails",
                    ExpectedException = new ExpectedException(typeof(SecurityTokenDecompressionFailedException), "IDX10679:", typeof(SecurityTokenDecompressionFailedException))
                }
            };
        }

        [Fact]
        public void Defaults()
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            Assert.True(handler.CanValidateToken, "!handler.CanValidateToken");
            Assert.True(handler.CanWriteToken, "!handler.CanWriteToken");
            Assert.True(handler.TokenType == typeof(JwtSecurityToken), "handler.TokenType != typeof(JwtSecurityToken)");
            Assert.True(handler.SetDefaultTimesOnTokenCreation);
        }

        [Fact]
        public void MaximumTokenSizeInBytes()
        {
            var handler = new JwtSecurityTokenHandler() { MaximumTokenSizeInBytes = 100 };
            var ee = ExpectedException.ArgumentException(substringExpected: "IDX10209:");
            try
            {
                handler.ReadToken(EncodedJwts.Asymmetric_LocalSts);
                ee.ProcessNoException();
            }
            catch (Exception ex)
            {
                ee.ProcessException(ex);
            }
        }

        // Test checks to make sure that default times are correctly added to the token
        // upon token creation.
        [Fact]
        public void SetDefaultTimesOnTokenCreation()
        {
            TestUtilities.WriteHeader($"{this}.SetDefaultTimesOnTokenCreation");
            var context = new CompareContext();

            var tokenHandler = new JwtSecurityTokenHandler();
            var descriptorNoTimeValues = new SecurityTokenDescriptor()
            {
                Issuer = Default.Issuer,
                Audience = Default.Audience,
                SigningCredentials = KeyingMaterial.JsonWebKeyRsa256SigningCredentials
            };

            var token = tokenHandler.CreateJwtSecurityToken(descriptorNoTimeValues);
            var jwt = token as JwtSecurityToken;

            Assert.NotNull(jwt.Payload.Iat);
            Assert.NotNull(jwt.Payload.Nbf);
            Assert.NotNull(jwt.Payload.Exp);
        }

        [Fact]
        public void ValidateTokenReplay()
        {
            TestUtilities.ValidateTokenReplay(Default.AsymmetricJwt, new JwtSecurityTokenHandler(), Default.AsymmetricSignTokenValidationParameters);
        }

        [Theory, MemberData(nameof(SegmentTheoryData))]
        public void SegmentRead(JwtTheoryData theoryData)
        {
            var context = TestUtilities.WriteHeader($"{this}.SegmentRead", theoryData);
            try
            {
                theoryData.TokenHandler.ReadJwtToken(theoryData.Token);
                theoryData.ExpectedException.ProcessNoException(context);
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex, context);
            }

            TestUtilities.AssertFailIfErrors(context);
        }

        [Theory, MemberData(nameof(SegmentTheoryData))]
        public void SegmentCanRead(JwtTheoryData theoryData)
        {
            Assert.Equal(theoryData.CanRead, theoryData.TokenHandler.CanReadToken(theoryData.Token));
        }

        public static TheoryData<JwtTheoryData> SegmentTheoryData()
        {
            var theoryData = new TheoryData<JwtTheoryData>();

            JwtTestData.InvalidRegExSegmentsDataForReadToken("IDX12709:", theoryData);
            JwtTestData.InvalidNumberOfSegmentsData("IDX12709:", theoryData);
            JwtTestData.InvalidEncodedSegmentsData("", theoryData);
            JwtTestData.ValidEncodedSegmentsData(theoryData);

            return theoryData;
        }

        [Theory, MemberData(nameof(ValidateAudienceTheoryData))]
        public void ValidateAudience(JwtTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.ValidateAudience", theoryData);

            try
            {
                SecurityToken securityToken;
                theoryData.TokenHandler.ValidateToken((theoryData.SecurityToken as JwtSecurityToken).RawData, theoryData.ValidationParameters, out securityToken);
                theoryData.ExpectedException.ProcessNoException();
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex);
            }
        }

        public static TheoryData<JwtTheoryData> ValidateAudienceTheoryData
        {
            get
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                return new TheoryData<JwtTheoryData>
                {
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", "empty" } }),
                        TestId = "'Audience == null'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: null),
                        TokenHandler = new JwtSecurityTokenHandler(),
                        ValidationParameters = ValidateAudienceValidationParameters(null, null, null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", "empty" } }),
                        TestId = "'Audience == string.Empty'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: string.Empty),
                        TokenHandler = new JwtSecurityTokenHandler(),
                        ValidationParameters = ValidateAudienceValidationParameters(null, null, null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", "        " } }),
                        TestId = "'Audience == whitespace'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: "        "),
                        TokenHandler = new JwtSecurityTokenHandler(),
                        ValidationParameters = ValidateAudienceValidationParameters(null, null, null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10214", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", Default.Audience } }),
                        TestId = "'Audience == NotDefault.Audience'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(NotDefault.Audience, null, null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", Default.Audience } }),
                        TestId = "'ValidAudience && ValidAudiences == null'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(null, null, null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", Default.Audience } }),
                        TestId = "'ValidAudience empty, validAudiences empty'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(string.Empty, new List<string>(), null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", Default.Audience } }),
                        TestId = "'ValidAudience whitespace, validAudiences empty'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters("    ", new List<string>(), null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", Default.Audience } }),
                        TestId = "'ValidAudiences one null string'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(null, new List<string>{ (string)null }, null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208: Unable to validate audience. validationParameters.ValidAudience is null or whitespace ", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", Default.Audience } }),
                        TestId = "'ValidAudiences == string.empty'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(null, new List<string>{ string.Empty }, null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10208: Unable to validate audience. ", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", Default.Audience } }),
                        TestId = "'ValidAudience string.empty, validAudiences whitespace'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(string.Empty, new List<string>{ "    " }, null, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: "IDX10231", propertiesExpected: new Dictionary<string, object>{ { "InvalidAudience", Default.Audience } }),
                        TestId = "'AudienceValidator return false'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(Default.Audience, null, ValidationDelegates.AudienceValidatorReturnsFalse, true),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidAudienceException), substringExpected: $"{typeof(ValidationDelegates)}.AudienceValidatorThrows"),
                        TestId = "'AudienceValidator throws'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(Default.Audience, null, ValidationDelegates.AudienceValidatorThrows, true),
                    },
                    new JwtTheoryData
                    {
                        TestId = "'validateAudience == false, validAudience null, validAudiences == null'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(null, null, null, false),
                    },
                    new JwtTheoryData
                    {
                        TestId = "'validateAudience == false, AudienceValidator throws'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(null, null, ValidationDelegates.AudienceValidatorThrows, false),
                    },
                    new JwtTheoryData
                    {
                        TestId = "'validateAudience == false, AudienceValidator return false'",
                        SecurityToken = tokenHandler.CreateJwtSecurityToken(issuer: Default.Issuer, audience: Default.Audience),
                        ValidationParameters = ValidateAudienceValidationParameters(null, null, ValidationDelegates.AudienceValidatorReturnsFalse, false),
                    }
                };
            }
        }

        private static TokenValidationParameters ValidateAudienceValidationParameters(string validAudience, IEnumerable<string> validAudiences, AudienceValidator audienceValidator, bool validateAudience)
        {
            return new TokenValidationParameters
            {
                AudienceValidator = audienceValidator,
                RequireSignedTokens = false,
                ValidateAudience = validateAudience,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidAudience = validAudience,
                ValidIssuers = validAudiences
            };
        }

        [Theory, MemberData(nameof(ValidateIssuerTheoryData))]
        public void ValidateIssuer(JwtTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.ValidateIssuer", theoryData);

            TestUtilities.ValidateToken(theoryData.Token, theoryData.ValidationParameters, theoryData.TokenHandler, theoryData.ExpectedException);
        }

        public static TheoryData<JwtTheoryData> ValidateIssuerTheoryData
        {
            get
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.CreateEncodedJwt(Default.Issuer, Default.Audience, Default.ClaimsIdentity, null, null, null, null);
                var properties = new Dictionary<string, object>
                {
                    {"InvalidIssuer", Default.Issuer }
                };


                return new TheoryData<JwtTheoryData>
                {
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidIssuerException), substringExpected: "IDX10204", propertiesExpected: properties),
                        Token = jwt,
                        TestId = "ValidIssuer == null, ValidIssuers == null",
                        ValidationParameters = ValidateIssuerValidationParameters(null, null, null, true)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidIssuerException), substringExpected: "IDX10205", propertiesExpected: properties),
                        TestId = "ValidIssuers = List<string>()",
                        Token = jwt,
                        ValidationParameters = ValidateIssuerValidationParameters(null, new List<string>(), null, true)
                    },
                    new JwtTheoryData
                    {
                        TestId = "NotDefault.Issuer: ValidateIssuer: false",
                        Token = jwt,
                        ValidationParameters = ValidateIssuerValidationParameters(NotDefault.Issuer, null, null, false)
                    },
                    new JwtTheoryData
                    {
                        TestId = "NotDefault.Issuers: ValidateIssuer: false",
                        Token = jwt,
                        ValidationParameters = ValidateIssuerValidationParameters(null, NotDefault.Issuers, null, false)
                    },
                    new JwtTheoryData
                    {
                        TestId = "ValidationDelegates.IssuerValidatorEcho",
                        Token = jwt,
                        ValidationParameters = ValidateIssuerValidationParameters(null, null, ValidationDelegates.IssuerValidatorEcho, true)
                    },
                    new JwtTheoryData
                    {
                        TestId = "Default.Issuer",
                        Token = jwt,
                        ValidationParameters = ValidateIssuerValidationParameters(null, Default.Issuers, null, true)
                    },
                    new JwtTheoryData
                    {
                        TestId = "Default.Issuers",
                        Token = jwt,
                        ValidationParameters = ValidateIssuerValidationParameters(null, Default.Issuers, null, true)
                    },
                    new JwtTheoryData
                    {
                        TestId = "ValidationDelegates.IssuerValidatorThrows, ValidateIssuer: false",
                        Token = jwt,
                        ValidationParameters = ValidateIssuerValidationParameters(
                            Default.Issuer,
                            Default.Issuers,
                            ValidationDelegates.IssuerValidatorThrows,
                            false)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidIssuerException), "IssuerValidatorThrows"),
                        Token = jwt,
                        TestId = "ValidationDelegates.IssuerValidatorThrows, ValidateIssuer: true",
                        ValidationParameters = ValidateIssuerValidationParameters(
                            Default.Issuer,
                            Default.Issuers,
                            ValidationDelegates.IssuerValidatorThrows,
                            true)
                    },
                };
            }
        }

        [Theory, MemberData(nameof(TokenReplayValidationTheoryData))]
        public void TokenReplayValidation(TokenReplayTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.TokenReplayValidation", theoryData);
            var context = new CompareContext($"{this}.ReadKeyInfo, {theoryData.TestId}");
            var tvp = Default.AsymmetricEncryptSignTokenValidationParameters.Clone();
            tvp.TokenReplayValidator = theoryData.TokenReplayValidator;
            tvp.ValidateTokenReplay = theoryData.ValidateTokenReplay;
            var token = Default.AsymmetricJwt;
            var tokenValidator = new JwtSecurityTokenHandler();

            try
            {
                if (theoryData.TokenReplayValidator == null)
                {
                    // TokenReplayCache is used since TokenReplayValidator is not provided.
                    // This test tests TokenReplayCache.
                    TestUtilities.ValidateTokenReplay(token, tokenValidator, tvp);
                }
                else
                {
                    // TokenReplayValidator is provided.
                    // This test tests TokenReplayValidator.
                    tokenValidator.ValidateToken(token, tvp, out SecurityToken validatedToken);
                    theoryData.ExpectedException.ProcessNoException(context.Diffs);
                }
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex, context.Diffs);
            }

            TestUtilities.AssertFailIfErrors(context);
        }

        public static TheoryData<TokenReplayTheoryData> TokenReplayValidationTheoryData
        {
            get
            {
                return TestTheoryData.TokenReplayValidationTheoryData;
            }
        }

        private static TokenValidationParameters ValidateIssuerValidationParameters(string validIssuer, IEnumerable<string> validIssuers, IssuerValidator issuerValidator, bool validateIssuer)
        {
            return new TokenValidationParameters
            {
                IssuerValidator = issuerValidator,
                RequireSignedTokens = false,
                ValidateAudience = false,
                ValidateIssuer = validateIssuer,
                ValidateLifetime = false,
                ValidIssuer = validIssuer,
                ValidIssuers = validIssuers
            };
        }

        [Theory, MemberData(nameof(ValidateLifetimeTheoryData))]
        public void ValidateLifetime(JwtTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.ValidateLifetime", theoryData);

            TestUtilities.ValidateToken(theoryData.Token, theoryData.ValidationParameters, theoryData.TokenHandler, theoryData.ExpectedException);
        }

        public static TheoryData<JwtTheoryData> ValidateLifetimeTheoryData
        {
            get
            {
                return new TheoryData<JwtTheoryData>
                {
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidLifetimeException("IDX10230:"),
                        TestId = nameof(ValidationDelegates.LifetimeValidatorReturnsFalse),
                        Token = Default.UnsignedJwt,
                        ValidationParameters = ValidateLifetimeValidationParameters(ValidationDelegates.LifetimeValidatorReturnsFalse, true)
                    },
                    new JwtTheoryData
                    {
                        TestId = $"{nameof(ValidationDelegates.LifetimeValidatorReturnsFalse)}, ValidateLifetime: false",
                        Token = Default.UnsignedJwt,
                        ValidationParameters = ValidateLifetimeValidationParameters(ValidationDelegates.LifetimeValidatorReturnsFalse, false)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidLifetimeException("IDX10230:"),
                        TestId = nameof(ValidationDelegates.LifetimeValidatorThrows),
                        Token = Default.UnsignedJwt,
                        ValidationParameters = ValidateLifetimeValidationParameters(ValidationDelegates.LifetimeValidatorReturnsFalse, true)
                    },
                    new JwtTheoryData
                    {
                        TestId = $"'{nameof(ValidationDelegates.LifetimeValidatorThrows)}, ValidateLifetime: false'",
                        Token = Default.UnsignedJwt,
                        ValidationParameters = ValidateLifetimeValidationParameters(ValidationDelegates.LifetimeValidatorThrows, false)
                    },
                };
            }
        }

        private static TokenValidationParameters ValidateLifetimeValidationParameters(LifetimeValidator lifetimeValidator, bool validateLifetime)
        {
            return new TokenValidationParameters
            {
                LifetimeValidator = lifetimeValidator,
                RequireSignedTokens = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = validateLifetime
            };
        }

        [Theory, MemberData(nameof(ValidateSignatureTheoryData))]
        public void ValidateSignature(JwtTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.ValidateSignature", theoryData);

            TestUtilities.ValidateToken(theoryData.Token, theoryData.ValidationParameters, theoryData.TokenHandler, theoryData.ExpectedException);
        }

        public static TheoryData<JwtTheoryData> ValidateSignatureTheoryData
        {
            get
            {
                var theoryData = new TheoryData<JwtTheoryData>
                {
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException(substringExpected: "IDX10503:"),
                        TestId = "Security Key Identifier not found",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_2048, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.X509SecurityKey_LocalSts, null)
                    },
                    new JwtTheoryData
                    {
                        TestId = "Asymmetric_LocalSts",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_LocalSts, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.X509SecurityKey_LocalSts, null)
                    },
                    new JwtTheoryData
                    {
                        TestId = "SigningKey null, SigningKeys single key",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_LocalSts, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, new List<SecurityKey> { KeyingMaterial.X509SecurityKey_LocalSts })
                    },
                    new JwtTheoryData
                    {
                        TestId = "Asymmetric_1024",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.X509SecurityKey_1024, null)
                    },
                    new JwtTheoryData
                    {
                        TestId = "'kid' is missing, 'x5t' is present.",
                        Token = EncodedJwts.JwsKidNullX5t,
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.DefaultX509Key_2048, null)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException(substringExpected: "IDX10504:"),
                        TestId = "Signature missing, required",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_2048, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.DefaultX509Key_2048_Public, null)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenSignatureKeyNotFoundException(substringExpected: "IDX10500:"),
                        TestId = "SigningKey and SigningKeys both null",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_2048, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenSignatureKeyNotFoundException(substringExpected: "IDX10500:"),
                        TestId = "SigningKeys empty",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_LocalSts, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, new List<SecurityKey>())
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException(substringExpected: "IDX10504:"),
                        TestId = "signature missing, RequireSignedTokens = true",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.NoExceptionExpected,
                        TestId = "signature missing, RequireSignedTokens = false",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null, false)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.NoExceptionExpected,
                        TestId = "custom signature validator",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null, true, ValidationDelegates.SignatureValidatorReturnsJwtTokenAsIs)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException(substringExpected: "IDX10505:"),
                        TestId = "signature validator returns null",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null, true, ValidationDelegates.SignatureValidatorReturnsNull)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException("SignatureValidatorThrows"),
                        TestId = "Signature validator throws",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null, true, ValidationDelegates.SignatureValidatorThrows)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.NoExceptionExpected,
                        TestId = "custom TokenReader",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Symmetric_256, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.DefaultSymmetricSecurityKey_256, null, true, null, ValidationDelegates.TokenReaderReturnsJwtSecurityToken)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException(substringExpected: "IDX10509:"),
                        TestId = "TokenReader returns incorrect token type",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null, true, null, ValidationDelegates.TokenReaderReturnsIncorrectSecurityTokenType)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException(substringExpected: "IDX10510:"),
                        TestId = "TokenReader returns null",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null, true, null, ValidationDelegates.TokenReaderReturnsNull)
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException("TokenReaderThrows"),
                        TestId = "TokenReader throws",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Asymmetric_1024, "Parts-0-1"),
                        ValidationParameters = ValidateSignatureValidationParameters(null, null, true, null, ValidationDelegates.TokenReaderThrows)
                    },
                    new JwtTheoryData
                    {
                        TestId = "EncodedJwts.Symmetric_256",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Symmetric_256, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.DefaultSymmetricSecurityKey_256, null),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException(substringExpected: "IDX10503:"),
                        TestId = "BinaryKey 56Bits",
                        Token = JwtTestUtilities.GetJwtParts(EncodedJwts.Symmetric_256, "ALLParts"),
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.DefaultSymmetricSecurityKey_56, null),
                    },
                    new JwtTheoryData
                    {
                        TestId = nameof(ValidationDelegates.IssuerSecurityKeyValidatorReturnsTrue),
                        Token = Default.AsymmetricJwt,
                        ValidationParameters = Default.AsymmetricSignTokenValidationParameters
                    },
                    new JwtTheoryData
                    {
                        TestId = "TVP.IssuerSigningKey.KeyId is uppercase and the kid found in the token is lowercase",
                        Token = EncodedJwts.JwsKidLowercase,
                        ValidationParameters = ValidateSignatureValidationParameters(KeyingMaterial.DefaultX509Key_2048, null)
                    },
                };

                var expectedException = ExpectedException.SecurityTokenInvalidSigningKeyException("IDX10232:");
                expectedException.PropertiesExpected.Add("SigningKey", Default.AsymmetricSigningKey);
                var validationParameters = Default.AsymmetricSignTokenValidationParameters;
                validationParameters.ValidateIssuerSigningKey = true;
                validationParameters.IssuerSigningKeyValidator = ValidationDelegates.IssuerSecurityKeyValidatorReturnsFalse;
                theoryData.Add(new JwtTheoryData
                {
                    ExpectedException = expectedException,
                    TestId = nameof(ValidationDelegates.IssuerSecurityKeyValidatorReturnsFalse),
                    Token = Default.AsymmetricJwt,
                    ValidationParameters = validationParameters
                });

                validationParameters = Default.AsymmetricSignTokenValidationParameters;
                validationParameters.CryptoProviderFactory = new CustomCryptoProviderFactory() { SigningSignatureProvider = new CustomSignatureProvider(KeyingMaterial.DefaultX509Key_2048, SecurityAlgorithms.RsaSha256) { VerifyResult = false } };
                theoryData.Add(new JwtTheoryData
                {
                    ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException("IDX10511:"),
                    TestId = $"{nameof(validationParameters.CryptoProviderFactory)} : returns false",
                    Token = Default.AsymmetricJwt,
                    ValidationParameters = validationParameters
                });

                return theoryData;
            }
        }

        private static TokenValidationParameters ValidateSignatureValidationParameters(SecurityKey signingKey, IEnumerable<SecurityKey> signingKeys)
        {
            return ValidateSignatureValidationParameters(signingKey, signingKeys, true);
        }

        private static TokenValidationParameters ValidateSignatureValidationParameters(SecurityKey signingKey, IEnumerable<SecurityKey> signingKeys, bool requireSignedTokens)
        {
            return ValidateSignatureValidationParameters(signingKey, signingKeys, requireSignedTokens, null);
        }

        private static TokenValidationParameters ValidateSignatureValidationParameters(SecurityKey signingKey, IEnumerable<SecurityKey> signingKeys, bool requireSignedTokens, SignatureValidator signatureValidator)
        {
            return ValidateSignatureValidationParameters(signingKey, signingKeys, requireSignedTokens, signatureValidator, null);
        }

        private static TokenValidationParameters ValidateSignatureValidationParameters(SecurityKey signingKey, IEnumerable<SecurityKey> signingKeys, bool requireSignedTokens, SignatureValidator signatureValidator, TokenReader tokenReader)
        {
            return new TokenValidationParameters()
            {
                IssuerSigningKey = signingKey,
                IssuerSigningKeys = signingKeys,
                RequireExpirationTime = false,
                RequireSignedTokens = requireSignedTokens,
                SignatureValidator = signatureValidator,
                TokenReader = tokenReader,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false
            };
        }

        [Theory, MemberData(nameof(ValidateTokenTheoryData))]
        public void ValidateToken(JwtTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.ValidateToken", theoryData);

            TestUtilities.ValidateToken(theoryData.Token, theoryData.ValidationParameters, theoryData.TokenHandler, theoryData.ExpectedException);
        }

        public static TheoryData<JwtTheoryData> ValidateTokenTheoryData
        {
            get
            {
                return new TheoryData<JwtTheoryData>
                {
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.ArgumentNullException(),
                        TestId = "Token: null",
                        ValidationParameters = new TokenValidationParameters()
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.ArgumentException(substringExpected: "IDX10209:"),
                        TestId = "Token.length > MaximumTokenSizeInBytes",
                        Token = EncodedJwts.Asymmetric_LocalSts,
                        TokenHandler = new JwtSecurityTokenHandler{ MaximumTokenSizeInBytes = 100 },
                        ValidationParameters = new TokenValidationParameters()
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.ArgumentException(substringExpected: "IDX12741:"),
                        TestId = "Token = Guid().NewGuid().ToString()",
                        Token = Guid.NewGuid().ToString(),
                        ValidationParameters = new TokenValidationParameters()
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.ArgumentNullException(),
                        TestId = "Token = '      ' (whitespace)",
                        Token = "     ",
                        ValidationParameters = new TokenValidationParameters()
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.ArgumentNullException(),
                        TestId = "TokenValidationParameters: null",
                        Token = EncodedJwts.Asymmetric_1024,
                    },
                    new JwtTheoryData
                    {
                        TestId = nameof(Default.AsymmetricJwt),
                        Token = Default.AsymmetricJwt,
                        ValidationParameters = Default.AsymmetricSignTokenValidationParameters
                    },
                    new JwtTheoryData
                    {
                        TestId = nameof(Default.SymmetricJws),
                        Token = Default.SymmetricJws,
                        ValidationParameters = Default.SymmetricSignTokenValidationParameters
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenInvalidSignatureException(substringExpected: "IDX10508:", innerTypeExpected: typeof(FormatException)),
                        TestId = "Token: Invalid Format",
                        Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.f",
                        ValidationParameters = new TokenValidationParameters()
                    },
                    new JwtTheoryData
                    {
                        TestId = nameof(Default.SymmetricJws) + "RequireSignedTokens",
                        Token = Default.SymmetricJws,
                        ValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            RequireSignedTokens = true,
                            IssuerSigningKey = Default.SymmetricSigningKey,
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                        }
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = ExpectedException.SecurityTokenSignatureKeyNotFoundException("IDX10501:"),
                        TestId = nameof(Default.SymmetricJws) + "RequireSignedTokensNullSigningKey",
                        Token = Default.SymmetricJws,
                        ValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            RequireSignedTokens = true,
                            IssuerSigningKey = null,
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                        }
                    },
                    new JwtTheoryData
                    {
                        TestId = nameof(Default.SymmetricJws) + "DontRequireSignedTokens",
                        Token = Default.SymmetricJws,
                        ValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            RequireSignedTokens = false,
                            IssuerSigningKey = Default.SymmetricSigningKey,
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                        }
                    },
                    new JwtTheoryData
                    {
                        TestId = nameof(Default.UnsignedJwt) + "DontRequireSignedTokensNullSigningKey",
                        Token = Default.UnsignedJwt,
                        ValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            RequireSignedTokens = false,
                            IssuerSigningKey = null,
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                        }
                    }
                };
            }
        }


        [Theory, MemberData(nameof(ValidateTypeTheoryData))]
        public void ValidateType(JwtTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.ValidateType", theoryData);

            try
            {
                theoryData.TokenHandler.ValidateToken(theoryData.Token, theoryData.ValidationParameters, out SecurityToken securityToken);
                theoryData.ExpectedException.ProcessNoException();
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex);
            }
        }

        public static TheoryData<JwtTheoryData> ValidateTypeTheoryData
        {
            get
            {
                // need to use JsonWebTokenHandler to create tokens with a different 'typ' value, as JwtSecurityTokenHandler currently does not have support for additional header claims.
                var jsonWebTokenHandler = new JsonWebTokenHandler();
                var type = "DifferentType";
                var jwsWithEmptyType = jsonWebTokenHandler.CreateToken(Default.PayloadString, new Dictionary<string, object>() { { JwtHeaderParameterNames.Typ, "" } });
                var jwsWithDifferentType = jsonWebTokenHandler.CreateToken(Default.PayloadString, new Dictionary<string, object>() { { JwtHeaderParameterNames.Typ, type } });
                var jweWithDifferentType = jsonWebTokenHandler.EncryptToken(jwsWithDifferentType, Default.SymmetricEncryptingCredentials);
                return new TheoryData<JwtTheoryData>
                {
                    new JwtTheoryData
                    {
                        TestId = "TypeEmptyValidTypesNull",
                        Token = jwsWithEmptyType,
                        ValidationParameters = ValidateTypeValidationParameters(null, null),
                    },
                    new JwtTheoryData
                    {
                        TestId = "TypeEmptyValidTypesEmpty",
                        Token = jwsWithEmptyType,
                        ValidationParameters = ValidateTypeValidationParameters(new List<string>(), null),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidTypeException), substringExpected: "IDX10256", propertiesExpected: new Dictionary<string, object>{ { "InvalidType", null } }),
                        TestId = "TypeEmptyValidTypesNonEmpty",
                        Token = jwsWithEmptyType,
                        ValidationParameters = ValidateTypeValidationParameters(new List<string>() { "Type" }, null),
                    },
                    new JwtTheoryData
                    {
                        TestId = "TypeNotEmptyValidTypesNull",
                        Token = jwsWithDifferentType,
                        ValidationParameters = ValidateTypeValidationParameters(null, null),
                    },
                    new JwtTheoryData
                    {
                        TestId = "TypeNotEmptyValidTypesEmpty",
                        Token = jwsWithDifferentType,
                        ValidationParameters = ValidateTypeValidationParameters(new List<string>(), null),
                    },
                    new JwtTheoryData
                    {
                        TestId = "JWSValidTypesNonEmptyValid",
                        Token = jwsWithDifferentType,
                        ValidationParameters = ValidateTypeValidationParameters(new List<string>() { type }, null),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidTypeException), substringExpected: "IDX10257", propertiesExpected: new Dictionary<string, object>{ { "InvalidType", type } }),
                        TestId = "JWSValidTypesNonEmptyInvalid",
                        Token = jwsWithDifferentType,
                        ValidationParameters = ValidateTypeValidationParameters(new List<string>() { type.ToUpper() }, null),
                    },
                    new JwtTheoryData
                    {
                        TestId = "JWEValidTypesNonEmptyValid",
                        Token = jweWithDifferentType,
                        ValidationParameters = ValidateTypeValidationParameters(new List<string>() { type }, Default.SymmetricEncryptingCredentials.Key),
                    },
                    new JwtTheoryData
                    {
                        ExpectedException = new ExpectedException(typeof(SecurityTokenInvalidTypeException), substringExpected: "IDX10257", propertiesExpected: new Dictionary<string, object>{ { "InvalidType", type } }),
                        TestId = "JWEValidTypesNonEmptyInvalid",
                        Token = jweWithDifferentType,
                        ValidationParameters = ValidateTypeValidationParameters(new List<string>() { type.ToUpper() }, Default.SymmetricEncryptingCredentials.Key),
                    }
                };
            }
        }

        private static TokenValidationParameters ValidateTypeValidationParameters(IEnumerable<string> validTypes, SecurityKey tokenDecryptionKey)
        {
            return new TokenValidationParameters
            {
                RequireSignedTokens = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidTypes = validTypes,
                TokenDecryptionKey = tokenDecryptionKey
            };
        }

        class ClaimComparer : IEqualityComparer<Claim>
        {
            public bool Equals(Claim x, Claim y)
            {
                if (x.Type == y.Type && x.Value == y.Value)
                    return true;

                return false;
            }

            public int GetHashCode(Claim obj)
            {
                throw new NotImplementedException();
            }
        }

        [Theory, MemberData(nameof(WriteTokenTheoryData))]
        public void WriteToken(JwtTheoryData theoryData)
        {
            try
            {
                var token = theoryData.TokenHandler.WriteToken(theoryData.SecurityToken);
                if (theoryData.TokenType == TokenType.JWE)
                    Assert.True(token.Split('.').Length == 5);
                else
                    Assert.True(token.Split('.').Length == 3);

                theoryData.ExpectedException.ProcessNoException();
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex);
            }
        }

        public static TheoryData<JwtTheoryData> WriteTokenTheoryData()
        {
            var theoryData = new TheoryData<JwtTheoryData>();

            theoryData.Add(new JwtTheoryData()
            {
                ExpectedException = ExpectedException.ArgumentNullException(),
                TestId = "Test1",
                SecurityToken = null
            });

            theoryData.Add(new JwtTheoryData
            {
                ExpectedException = ExpectedException.ArgumentException("IDX12706:"),
                TestId = "Test2",
                SecurityToken = new DerivedSecurityToken()
            });

            theoryData.Add(new JwtTheoryData
            {
                ExpectedException = ExpectedException.ArgumentException("IDX12706:"),
                TestId = "Test3",
                SecurityToken = new DerivedSecurityToken()
            });

            theoryData.Add(new JwtTheoryData
            {
                ExpectedException = ExpectedException.SecurityTokenEncryptionFailedException("IDX12736:"),
                TestId = "Test4",
                SecurityToken = new JwtSecurityToken(
                                new JwtHeader(Default.SymmetricSigningCredentials),
                                new JwtSecurityToken(),
                                "ey",
                                "ey",
                                "ey",
                                "ey",
                                "ey")
            });

            theoryData.Add(new JwtTheoryData
            {
                ExpectedException = ExpectedException.SecurityTokenEncryptionFailedException("IDX12735:"),
                TestId = "Test5",
                SecurityToken = new JwtSecurityToken(
                                new JwtHeader(),
                                new JwtSecurityToken(),
                                "ey",
                                "ey",
                                "ey",
                                "ey",
                                "ey")
            });

            var header = new JwtHeader(Default.SymmetricSigningCredentials);
            var payload = new JwtPayload();
            theoryData.Add(new JwtTheoryData
            {
                TestId = "Test6",
                SecurityToken = new JwtSecurityToken(
                    new JwtHeader(Default.SymmetricEncryptingCredentials),
                    new JwtSecurityToken(header, payload),
                    "ey",
                    "ey",
                    "ey",
                    "ey",
                    "ey"),
                TokenType = TokenType.JWE
            });

            theoryData.Add(new JwtTheoryData()
            {
                TestId = "Test7",
                SecurityToken = new JwtSecurityToken(
                    new JwtHeader(Default.SymmetricSigningCredentials),
                    new JwtPayload() ),
                TokenType = TokenType.JWS
            });

            header = new JwtHeader(Default.SymmetricSigningCredentials);
            payload = new JwtPayload();
            var innerToken = new JwtSecurityToken(
                    header,
                    new JwtSecurityToken(header, payload),
                    "ey",
                    "ey",
                    "ey",
                    "ey",
                    "ey");

            theoryData.Add(new JwtTheoryData
            {
                TestId = "Test8",
                SecurityToken = new JwtSecurityToken(
                        new JwtHeader(Default.SymmetricEncryptingCredentials),
                            innerToken,
                            "ey",
                            "ey",
                            "ey",
                            "ey",
                            "ey"),
                TokenType = TokenType.JWE
            });

            return theoryData;
        }

        [Theory, MemberData(nameof(KeyWrapTokenTheoryData))]
        public void KeyWrapTokenTest(KeyWrapTokenTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.KeyWrapTokenTest", theoryData);

            try
            {
                var signingCredentials = KeyingMaterial.DefaultSymmetricSigningCreds_256_Sha2;
                var securityTokenDescriptor = Default.SecurityTokenDescriptor(theoryData.EncryptingCredentials, signingCredentials, null);

                var handler = new JwtSecurityTokenHandler();
                var token = handler.CreateToken(securityTokenDescriptor);
                var tokenString = handler.WriteToken(token);

                var validationParameters = Default.TokenValidationParameters(theoryData.DecryptingCredentials.Key, signingCredentials.Key);
                var principal = handler.ValidateToken(tokenString, validationParameters, out var validatedToken);

                Assert.NotNull(principal);
                theoryData.ExpectedException.ProcessNoException();
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex);
            }
        }

        public static TheoryData<KeyWrapTokenTheoryData> KeyWrapTokenTheoryData()
        {
            var theoryData = new TheoryData<KeyWrapTokenTheoryData>();
            var handler = new JwtSecurityTokenHandler();
            var rsaOAEPEncryptingCredential = new EncryptingCredentials(KeyingMaterial.DefaultX509Key_2048, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512);
            var rsaPKCS1EncryptingCredential = new EncryptingCredentials(KeyingMaterial.DefaultX509Key_2048, SecurityAlgorithms.RsaPKCS1, SecurityAlgorithms.Aes256CbcHmacSha512);

            theoryData.Add(new KeyWrapTokenTheoryData
            {
                EncryptingCredentials = rsaOAEPEncryptingCredential,
                DecryptingCredentials = rsaOAEPEncryptingCredential,
                TestId = "Key wrap token test using OAEP padding"
            });

            theoryData.Add(new KeyWrapTokenTheoryData
            {
                EncryptingCredentials = rsaPKCS1EncryptingCredential,
                DecryptingCredentials = rsaPKCS1EncryptingCredential,
                TestId = "Key wrap token test using PKCS1 padding"
            });

            theoryData.Add(new KeyWrapTokenTheoryData
            {
                EncryptingCredentials = rsaPKCS1EncryptingCredential,
                DecryptingCredentials = Default.SymmetricEncryptingCredentials,
                ExpectedException = ExpectedException.SecurityTokenDecryptionFailedException("IDX10609:"),
                TestId = "Key wrap token test using RSA to wrap but symmetric key to unwrap"
            });

            theoryData.Add(new KeyWrapTokenTheoryData
            {
                EncryptingCredentials = Default.SymmetricEncryptingCredentials,
                DecryptingCredentials = rsaPKCS1EncryptingCredential,
                ExpectedException = ExpectedException.SecurityTokenDecryptionFailedException("IDX10609:"),
                TestId = "Key wrap token test using symmetric key to wrap but RSA to unwrap"
            });

            return theoryData;
        }

        [Theory, MemberData(nameof(ParametersCheckTheoryData))]
        public void ParametersCheckTest(ParametersCheckTheoryData theoryData)
        {
            TestUtilities.WriteHeader($"{this}.ParametersCheckTest", theoryData);

            var handler = new JwtSecurityTokenHandlerCustom();
            try
            {
                handler.CreateClaimsIdentityCustom(theoryData.token, "issuer", theoryData.validationParameters);
                theoryData.ExpectedException.ProcessNoException();
            }
            catch(Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex);
            }
        }

        public static TheoryData<ParametersCheckTheoryData> ParametersCheckTheoryData()
        {
            return new TheoryData<ParametersCheckTheoryData>()
            {
                new ParametersCheckTheoryData
                {
                    TestId = "Missing token",
                    token = null,
                    ExpectedException = ExpectedException.ArgumentNullException("IDX10000")
                },
                new ParametersCheckTheoryData
                {
                    TestId = "Missing validationParameters",
                    validationParameters = null,
                    ExpectedException = ExpectedException.ArgumentNullException("IDX10000")
                }
            };
        }
    }

    public class KeyWrapTokenTheoryData : TheoryDataBase
    {
        public EncryptingCredentials EncryptingCredentials;
        public EncryptingCredentials DecryptingCredentials;
    }

    public enum TokenType
    {
        JWE,
        JWS
    }

    public class JwtSecurityTokenHandlerCustom : JwtSecurityTokenHandler
    {
        public void CreateClaimsIdentityCustom(JwtSecurityToken jwtToken, string issuer, TokenValidationParameters validationParameters)
        {
            CreateClaimsIdentity(jwtToken, issuer, validationParameters);
        }
    }

    public class ParametersCheckTheoryData : TheoryDataBase
    {
        public JwtSecurityToken token { get; set; } = new JwtSecurityToken();
        public TokenValidationParameters validationParameters { get; set; } = new TokenValidationParameters();
    }
}

#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
