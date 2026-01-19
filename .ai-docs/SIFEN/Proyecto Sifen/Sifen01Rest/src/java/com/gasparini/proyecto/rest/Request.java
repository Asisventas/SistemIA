/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.gasparini.proyecto.rest;

/**
 *
 * @author Admin
 */
public class Request {
    
    private String archivoOrigen;
    private String archivoDestino;
    private String archivoCertificado;
    private String passwordCertificado;
    private String urlWebService;
    private String archivoCrt;
    private String archivoP12;

    public String getArchivoP12() {
        return archivoP12;
    }

    public void setArchivoP12(String archivoP12) {
        this.archivoP12 = archivoP12;
    }

    public Request() {
    }

    public String getArchivoOrigen() {
        return archivoOrigen;
    }

    public void setArchivoOrigen(String archivoOrigen) {
        this.archivoOrigen = archivoOrigen;
    }

    public String getArchivoDestino() {
        return archivoDestino;
    }

    public void setArchivoDestino(String archivoDestino) {
        this.archivoDestino = archivoDestino;
    }

    public String getArchivoCertificado() {
        return archivoCertificado;
    }

    public void setArchivoCertificado(String archivoCertificado) {
        this.archivoCertificado = archivoCertificado;
    }

    public String getPasswordCertificado() {
        return passwordCertificado;
    }

    public void setPasswordCertificado(String passwordCertificado) {
        this.passwordCertificado = passwordCertificado;
    }

    public String getUrlWebService() {
        return urlWebService;
    }

    public void setUrlWebService(String urlWebService) {
        this.urlWebService = urlWebService;
    }

    public String getArchivoCrt() {
        return archivoCrt;
    }

    public void setArchivoCrt(String archivoCrt) {
        this.archivoCrt = archivoCrt;
    }
    
    
}
