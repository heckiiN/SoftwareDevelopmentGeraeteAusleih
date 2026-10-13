namespace Geraeteausleihe.Core.Domain;

// Verstöße gegen Fachregeln. Die Meldung wird so in der Oberfläche angezeigt.
public class FachlicherFehler(string meldung) : Exception(meldung);
