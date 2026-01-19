package com.gasparini.proyecto.rest;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.IOException;
import java.io.OutputStream;
import java.io.UnsupportedEncodingException;
import java.math.BigInteger;
import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.net.URL;
import java.security.cert.CertificateException;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;
import java.security.InvalidAlgorithmParameterException;
import java.security.KeyFactory;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.security.PrivateKey;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.PKCS8EncodedKeySpec;
import java.util.Base64;
import java.util.Collections;
import java.util.LinkedList;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;
import javax.net.ssl.HttpsURLConnection;
import javax.ws.rs.Consumes;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.Path;
import javax.ws.rs.POST;
import javax.ws.rs.Produces;
import javax.xml.crypto.dsig.CanonicalizationMethod;
import javax.xml.crypto.dsig.DigestMethod;
import javax.xml.crypto.dsig.dom.DOMSignContext;
import javax.xml.crypto.dsig.keyinfo.KeyInfo;
import javax.xml.crypto.dsig.keyinfo.KeyInfoFactory;
import javax.xml.crypto.dsig.keyinfo.X509Data;
import javax.xml.crypto.dsig.Reference;
import javax.xml.crypto.dsig.SignedInfo;
import javax.xml.crypto.dsig.spec.C14NMethodParameterSpec;
import javax.xml.crypto.dsig.spec.TransformParameterSpec;
import javax.xml.crypto.dsig.Transform;
import javax.xml.crypto.dsig.XMLSignature;
import javax.xml.crypto.dsig.XMLSignatureException;
import javax.xml.crypto.dsig.XMLSignatureFactory;
import javax.xml.crypto.MarshalException;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import org.json.JSONObject;
import org.json.JSONArray;
import org.json.JSONException;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.SAXException;

@Path("sifen")
public class SifenResource {

    public SifenResource() {
    }

    @POST
    @Path("firmado")
    @Consumes(MediaType.TEXT_PLAIN)
    @Produces(MediaType.TEXT_PLAIN)
    public String firmado(String request) {
        Request data = new Request();
        Response response = new Response();
        String qr = null;
        try {
            JSONArray array = new JSONArray(request);
            for (int i = 0; i < array.length(); i++) {
                try {
                    JSONObject object = array.getJSONObject(i);
                    data.setArchivoOrigen(object.getString("archivoOrigen"));
                    data.setArchivoDestino(object.getString("archivoDestino"));
                    data.setArchivoCertificado(object.getString("archivoCertificado"));
                    data.setPasswordCertificado(object.getString("passwordCertificado"));
                    data.setUrlWebService(object.getString("urlWebService"));
                    data.setArchivoCrt(object.getString("archivoCrt"));
                    data.setArchivoP12(object.getString("archivoP12"));
                } catch (JSONException ex) {
                    Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                }
            }
        } catch (JSONException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
        }
        try {
            System.setProperty("com.sun.org.apache.xml.internal.security.ignoreLineBreaks", "true");
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder builder = factory.newDocumentBuilder();
            File file = new File(data.getArchivoOrigen());
            Document doc = null;
            try {
                doc = builder.parse(file);
            } catch (SAXException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10001");
                response.setMensaje(ex.getMessage());
            } catch (IOException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10002");
                response.setMensaje(ex.getMessage());
            }
            Element parent = (Element) doc.getDocumentElement().getElementsByTagName("rDE").item(0);
            Element signedElement = (Element) parent.getElementsByTagName("DE").item(0);
            Element nextSibling = (Element) parent.getElementsByTagName("gCamFuFD").item(0);
            file = new File(data.getArchivoCertificado());
            String key = null;
            try {
                key = new String(Files.readAllBytes(file.toPath()), Charset.defaultCharset());
            } catch (IOException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10003");
                response.setMensaje(ex.getMessage());
            }
            String privateKeyPEM = key
                    .replace("-----BEGIN PRIVATE KEY-----", "")
                    .replaceAll(System.lineSeparator(), "")
                    .replace("-----END PRIVATE KEY-----", "");
            byte[] encoded = Base64.getDecoder().decode(privateKeyPEM);
            KeyFactory keyFactory = null;
            try {
                keyFactory = KeyFactory.getInstance("RSA");
            } catch (NoSuchAlgorithmException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10004");
                response.setMensaje(ex.getMessage());
            }
            PKCS8EncodedKeySpec keySpec = new PKCS8EncodedKeySpec(encoded);
            PrivateKey privateKey = null;
            try {
                privateKey = keyFactory.generatePrivate(keySpec);
            } catch (InvalidKeySpecException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10005");
                response.setMensaje(ex.getMessage());
            }
            XMLSignatureFactory xmlSignatureFactory = XMLSignatureFactory.getInstance();
            String requestId = signedElement.getAttribute("Id");
            signedElement.setIdAttribute("Id", true);
            List<Transform> transforms = new LinkedList<>();
            try {
                transforms.add(xmlSignatureFactory.newTransform(Transform.ENVELOPED, (TransformParameterSpec) null));
            } catch (NoSuchAlgorithmException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10006");
                response.setMensaje(ex.getMessage());
            } catch (InvalidAlgorithmParameterException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10007");
                response.setMensaje(ex.getMessage());
            }
            try {
                transforms.add(xmlSignatureFactory.newTransform(CanonicalizationMethod.EXCLUSIVE, (TransformParameterSpec) null));
            } catch (NoSuchAlgorithmException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10008");
                response.setMensaje(ex.getMessage());
            } catch (InvalidAlgorithmParameterException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10009");
                response.setMensaje(ex.getMessage());
            }
            Reference reference = null;
            try {
                reference = xmlSignatureFactory.newReference("#" + requestId, xmlSignatureFactory.newDigestMethod(DigestMethod.SHA256, null), transforms, null, null);
            } catch (NoSuchAlgorithmException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10010");
                response.setMensaje(ex.getMessage());
            } catch (InvalidAlgorithmParameterException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10011");
                response.setMensaje(ex.getMessage());
            }
            SignedInfo signedInfo = null;
            try {
                signedInfo = xmlSignatureFactory.newSignedInfo(
                        xmlSignatureFactory.newCanonicalizationMethod(CanonicalizationMethod.EXCLUSIVE, (C14NMethodParameterSpec) null),
                        xmlSignatureFactory.newSignatureMethod(Constants.RSA_SHA256, null),
                        Collections.singletonList(reference)
                );
            } catch (NoSuchAlgorithmException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10012");
                response.setMensaje(ex.getMessage());
            } catch (InvalidAlgorithmParameterException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10013");
                response.setMensaje(ex.getMessage());
            }
            CertificateFactory fact = null;
            try {
                fact = CertificateFactory.getInstance("X.509");
            } catch (CertificateException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10014");
                response.setMensaje(ex.getMessage());
            }
            FileInputStream is = null;
            try {
                is = new FileInputStream(data.getArchivoCrt());
            } catch (FileNotFoundException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10024");
                response.setMensaje(ex.getMessage());
            }
            X509Certificate keyInfoValue = null;
            try {
                keyInfoValue = (X509Certificate) fact.generateCertificate(is);
            } catch (CertificateException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10016");
                response.setMensaje(ex.getMessage());
            }
            KeyInfoFactory keyInfoFactory = xmlSignatureFactory.getKeyInfoFactory();
            X509Data x509Data = keyInfoFactory.newX509Data(Collections.singletonList(keyInfoValue));
            KeyInfo keyInfo = keyInfoFactory.newKeyInfo(Collections.singletonList(x509Data));
            XMLSignature signature = xmlSignatureFactory.newXMLSignature(signedInfo, keyInfo);
            DOMSignContext signatureContext = new DOMSignContext(privateKey, parent, nextSibling);
            try {
                signature.sign(signatureContext);
            } catch (MarshalException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10017");
                response.setMensaje(ex.getMessage());
            } catch (XMLSignatureException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10018");
                response.setMensaje(ex.getMessage());
            }
            OutputStream os = System.out;
            TransformerFactory tf = TransformerFactory.newInstance();
            Transformer trans = null;
            try {
                trans = tf.newTransformer();
            } catch (TransformerConfigurationException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10019");
                response.setMensaje(ex.getMessage());
            }
            NodeList nodeListDigest = doc.getElementsByTagName("DigestValue");
            String digestValue = nodeListDigest.item(0).getTextContent();
            NodeList nodeList = doc.getElementsByTagName("dCarQR");
            Node node = nodeList.item(0);
            Element tElement = (Element) node;
            if (tElement.getTextContent().indexOf("665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d") > 0) {
                try {
                    tElement.setTextContent(tElement.getTextContent().replace("665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d", String.format("%040x", new BigInteger(1, digestValue.getBytes("UTF-8")))));
                    MessageDigest digest = null;
                    try {
                        digest = MessageDigest.getInstance("SHA-256");
                        digest.update(tElement.getTextContent().getBytes(StandardCharsets.UTF_8));
                        byte[] encodedhash = digest.digest();
                        String hex = String.format("%064x", new BigInteger(1, encodedhash));
                        qr = "https://ekuatia.set.gov.py/consultas-test/qr?" + tElement.getTextContent().substring(0, (tElement.getTextContent().length() - 32)) + "&cHashQR=" + hex;
                        tElement.setTextContent(qr);
                    } catch (NoSuchAlgorithmException ex) {
                        Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                        response.setCodigo("10020");
                        response.setMensaje(ex.getMessage());
                    }
                } catch (UnsupportedEncodingException ex) {
                    Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                    response.setCodigo("10021");
                    response.setMensaje(ex.getMessage());
                }
            }
            try {
                trans.transform(new DOMSource(doc), new StreamResult(data.getArchivoDestino()));
                response = envio(data.getUrlWebService(),
                        data.getArchivoDestino(),
                        data.getArchivoCertificado(),
                        data.getPasswordCertificado(),
                        data.getArchivoP12());
            } catch (TransformerException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10022");
                response.setMensaje(ex.getMessage());
            }
        } catch (ParserConfigurationException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10023");
            response.setMensaje(ex.getMessage());
        }
        return " {\"codigo\":" + "\"" + response.getCodigo() + "\"," + "\"mensaje\":" + "\"" + response.getMensaje() + "\"," + "\"qr\":" + "\"" + qr + "\"}";
    }

    public Response envio(String urlWebService,
            String archivoDestino,
            String archivoCertificado,
            String passwordCertificado,
            String archivoP12) {
        Response response = new Response();
        System.setProperty("javax.net.debug", "ssl,handshake");
        System.setProperty("javax.net.ssl.keyStoreType", "pkcs12");
        System.setProperty("javax.net.ssl.keyStore", archivoP12);
        System.setProperty("javax.net.ssl.keyStorePassword", passwordCertificado);
        String url = urlWebService;
        URL obj = null;
        try {
            obj = new URL(url);
        } catch (MalformedURLException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10025");
            response.setMensaje(ex.getMessage());
        }
        HttpsURLConnection con = null;
        try {
            con = (HttpsURLConnection) obj.openConnection();
        } catch (IOException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10026");
            response.setMensaje(ex.getMessage());
        }
        try {
            con.setRequestMethod("POST");
        } catch (ProtocolException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10027");
            response.setMensaje(ex.getMessage());
        }
        con.setRequestProperty("Content-Type", "text/xml; charset=utf-8");
        con.setDoOutput(true);
        OutputStream out = null;
        try {
            out = con.getOutputStream();
        } catch (IOException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10028");
            response.setMensaje(ex.getMessage());
        }
        FileInputStream fileIn = null;
        try {
            fileIn = new FileInputStream(archivoDestino);
        } catch (FileNotFoundException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10029");
            response.setMensaje(ex.getMessage());
        }
        byte[] buffer = new byte[1024];
        int nbRead = 0;
        do {
            try {
                nbRead = fileIn.read(buffer);
            } catch (IOException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10030");
                response.setMensaje(ex.getMessage());
            }
            if (nbRead > 0) {
                try {
                    out.write(buffer, 0, nbRead);
                } catch (IOException ex) {
                    Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                    response.setCodigo("10031");
                    response.setMensaje(ex.getMessage());
                }
            }
        } while (nbRead >= 0);
        int returnCode = 0;
        try {
            returnCode = con.getResponseCode();
        } catch (IOException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10032");
            response.setMensaje(ex.getMessage());
        }
        InputStream connectionIn = null;
        if (returnCode == 200) {
            try {
                connectionIn = con.getInputStream();
            } catch (IOException ex) {
                Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
                response.setCodigo("10033");
                response.setMensaje(ex.getMessage());
            }
        } else {
            connectionIn = con.getErrorStream();
        }
        BufferedReader bufferReader = new BufferedReader(new InputStreamReader(connectionIn));
        String inputLine;
        try {
            while ((inputLine = bufferReader.readLine()) != null) {
                if (inputLine.indexOf("<ns2:dCodRes>") > 1) {
                    response.setCodigo(inputLine.substring((inputLine.indexOf("<ns2:dCodRes>") + 13), inputLine.indexOf("</ns2:dCodRes>")));
                    response.setMensaje(inputLine.substring((inputLine.indexOf("<ns2:dMsgRes>") + 13), inputLine.indexOf("</ns2:dMsgRes>")));
                }
            }
        } catch (IOException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10034");
            response.setMensaje(ex.getMessage());
        }
        try {
            bufferReader.close();
        } catch (IOException ex) {
            Logger.getLogger(SifenResource.class.getName()).log(Level.SEVERE, null, ex);
            response.setCodigo("10035");
            response.setMensaje(ex.getMessage());
        }
        return response;
    }
}
