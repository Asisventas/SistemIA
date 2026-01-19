/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.gasparini.proyecto.rest;

/**
 *
 * @author Administrador
 */
public class Mail {
    
    private String fromMail;
    private String fromPassword;
    private String toMail;
    private String subject;
    private String body;
    private String[] attach;

    public String getFromPassword() {
        return fromPassword;
    }

    public void setFromPassword(String fromPassword) {
        this.fromPassword = fromPassword;
    }

    public String getFromMail() {
        return fromMail;
    }

    public void setFromMail(String fromMail) {
        this.fromMail = fromMail;
    }

    public String getToMail() {
        return toMail;
    }

    public void setToMail(String toMail) {
        this.toMail = toMail;
    }

    public String getSubject() {
        return subject;
    }

    public void setSubject(String subject) {
        this.subject = subject;
    }

    public String getBody() {
        return body;
    }

    public void setBody(String body) {
        this.body = body;
    }

    /*public String getAttach() {
        return attach;
    }

    public void setAttach(String attach) {
        this.attach = attach;
    }*/
    
    
    
}
