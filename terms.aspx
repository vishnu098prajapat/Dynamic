<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="terms.aspx.cs" Inherits="terms" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        .legal-hero {
            padding: 140px 0 60px;
            text-align: center;
            background: var(--bg-section);
            position: relative;
        }

        .legal-badge {
            display: inline-block;
            padding: 0.5rem 1.5rem;
            background: var(--surface);
            border: 1px solid var(--border-color);
            border-radius: 30px;
            color: var(--gold);
            font-weight: 600;
            margin-bottom: 1.5rem;
            font-size: 0.9rem;
        }

        .legal-hero h1 {
            font-size: 3.5rem;
            margin-bottom: 1rem;
            color: var(--text-head);
        }

        .legal-hero p {
            font-size: 1.1rem;
            color: var(--text-body);
            max-width: 600px;
            margin: 0 auto;
        }

        .legal-content {
            max-width: 900px;
            margin: 0 auto;
            padding: 4rem 1.5rem;
            background: var(--surface);
            border: 1px solid var(--border-color);
            border-radius: 24px;
            margin-bottom: 4rem;
            color: var(--text-body);
            line-height: 1.8;
            font-size: 1.05rem;
        }

            .legal-content h2 {
                font-size: 2rem;
                color: var(--gold);
                margin-top: 2.5rem;
                margin-bottom: 1.5rem;
            }

                .legal-content h2:first-child {
                    margin-top: 0;
                }

            .legal-content p {
                margin-bottom: 1.5rem;
            }

            .legal-content ul {
                margin-bottom: 1.5rem;
                padding-left: 2rem;
                list-style-type: disc;
            }

            .legal-content li {
                margin-bottom: 0.5rem;
            }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <section class="legal-hero">
        <div class="container">
            <div class="legal-badge">&#128220; Terms & Conditions</div>
            <h1 class="font-playfair">Terms of Service</h1>
            <p>Last updated: <%= DateTime.Now.ToString("MMMM yyyy") %> | <%=company.Name %></p>
        </div>
    </section>

    <section class="container">
        <div class="legal-content">
            <asp:Repeater runat="server" ID="rptDataTermsCondition">
                <ItemTemplate>
                   <p><%# Eval("Description") %></p> 
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </section>
</asp:Content>
